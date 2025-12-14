using FluentValidation;
using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Aplications.Aplication.Interfaces.Services;
using Fundo.Aplications.Aplication.UseCases.Auth;
using Fundo.Aplications.Aplication.UseCases.Loans;
using Fundo.Aplications.Aplication.Validators.Auth;
using Fundo.Aplications.Aplication.Validators.Loans;
using Fundo.Applications.Domain.Requests.Auth;
using Fundo.Applications.Domain.Requests.Loans;
using Fundo.Applications.Infra.Context;
using Fundo.Applications.Infra.Identity;
using Fundo.Applications.Infra.Repositories;
using Fundo.Applications.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", builder =>
                {
                    builder
                        .WithOrigins(
                            "http://localhost:4200",
                            "http://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });


            services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(
                Configuration.GetConnectionString("Default"),
                    x => x.MigrationsAssembly("Fundo.Applications.Infra")
            ));

            services.AddDbContext<AppIdentityDbContext>(opt =>
                opt.UseSqlServer(
                    Configuration.GetConnectionString("Default"),
                    x => x.MigrationsAssembly("Fundo.Applications.Infra")
                ));

            services
                .AddIdentity<AppUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                })
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            var jwtKey = Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("JWT Key was not configured.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Convert.FromBase64String(Configuration["Jwt:Key"]!)
                    ),

                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            services.AddAuthorization();

            services.AddScoped<ITokenServices, TokenService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<CreateUserUseCase>();
            services.AddScoped<LoginUserUseCase>();
            services.AddScoped<CreateLoanUseCase>();
            services.AddScoped<GetAllLoansUseCase>();
            services.AddScoped<GetLoanByIdUseCase>();
            services.AddScoped<PayLoanUseCase>();

            services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
            services.AddScoped<IValidator<LoginRequest>, LoginUserValidator>();
            services.AddScoped<IValidator<CreateLoanRequest>, CreateLoanValidator>();
            services.AddScoped<IValidator<GetLoanByIdRequest>, GetLoanByIdValidator>();
            services.AddScoped<IValidator<PayLoanRequest>, PayLoanValidator>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fundo API", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Informe: Bearer {seu token JWT}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fundo API v1");
                });
            }

            ApplyMigrationsWithRetry(app, app.ApplicationServices.GetRequiredService<ILogger<Startup>>());

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRouting();

            app.UseCors("AllowAngular");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }

        private static void ApplyMigrationsWithRetry(IApplicationBuilder app, Microsoft.Extensions.Logging.ILogger logger)
        {
            const int maxRetries = 10;

            using var scope = app.ApplicationServices.CreateScope();


            var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

            Migrate(appDb, logger, maxRetries);
            Migrate(identityDb, logger, maxRetries);

            static void Migrate(DbContext db, Microsoft.Extensions.Logging.ILogger logger, int maxRetries)
            {
                for (var attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        logger.LogInformation("Applying migrations for {DbContext}. Attempt {Attempt}/{MaxRetries}",
                            db.GetType().Name, attempt, maxRetries);

                        db.Database.Migrate();

                        logger.LogInformation("Migrations applied successfully for {DbContext}", db.GetType().Name);
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "Failed applying migrations for {DbContext}. Attempt {Attempt}/{MaxRetries}",
                            db.GetType().Name, attempt, maxRetries);

                        if (attempt == maxRetries) throw;

                        System.Threading.Thread.Sleep(2000);
                    }
                }
            }
        }

    }
}

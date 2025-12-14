using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fundo.Applications.Infra.Context; 

namespace Fundo.Services.Tests.Integration
{
    public sealed class CustomWebApplicationFactory
        : WebApplicationFactory<Applications.WebApi.Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.UseEnvironment("Development");

            builder.ConfigureServices((context, services) =>
            {

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });

                services.AddLogging(lb =>
                {
                    lb.ClearProviders();
                    lb.AddConsole();
                    lb.SetMinimumLevel(LogLevel.Debug);
                });

                var cs = context.Configuration.GetConnectionString("Default")
                         ?? context.Configuration["ConnectionStrings:Default"];

                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException("Connection string 'ConnectionStrings:Default' not found.");

                var dbCtx = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbCtx != null) services.Remove(dbCtx);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(cs));

 
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (!db.Database.CanConnect())
                    throw new InvalidOperationException("Cannot connect to SQL Server with the provided connection string.");

                db.Database.Migrate();
            });
        }
    }
}

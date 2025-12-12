using Fundo.Aplications.Aplication.Interfaces.Services;
using Fundo.Applications.Domain.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Infra.Identity
{
    public sealed class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<IdentityService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<AuthenticatedUser> CreateUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Identity: creating user. Email={Email}",
                email);

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser is not null)
            {
                _logger.LogWarning(
                    "Identity: user creation failed. Email already in use. Email={Email}",
                    email);

                throw new InvalidOperationException("Email is in use already");
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();

                _logger.LogWarning(
                    "Identity: user creation failed. Email={Email}, Errors={Errors}",
                    email,
                    errors);

                throw new InvalidOperationException(string.Join("; ", errors));
            }

            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation(
                "Identity: user created successfully. UserId={UserId}, Email={Email}",
                user.Id,
                email);

            return new AuthenticatedUser(
                user.Id,
                user.Email ?? email,
                roles.ToList());
        }

        public async Task<AuthenticatedUser> LoginUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Identity: login attempt. Email={Email}",
                email);

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                _logger.LogWarning(
                    "Identity: login failed. User not found. Email={Email}",
                    email);

                throw new UnauthorizedAccessException("Invalid Credentials");
            }

            var signInResult = await _signInManager
                .CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                _logger.LogWarning(
                    "Identity: login failed. Invalid password. Email={Email}",
                    email);

                throw new UnauthorizedAccessException("Invalid Credentials");
            }

            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation(
                "Identity: login succeeded. UserId={UserId}, Email={Email}",
                user.Id,
                email);

            return new AuthenticatedUser(
                user.Id,
                user.Email ?? email,
                roles.ToList());
        }
    }
}

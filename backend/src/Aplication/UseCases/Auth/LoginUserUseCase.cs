using FluentValidation;
using Fundo.Aplications.Aplication.Interfaces.Services;
using Fundo.Applications.Domain.Requests.Auth;
using Fundo.Applications.Domain.Responses.Auth;
using Microsoft.Extensions.Logging;

namespace Fundo.Aplications.Aplication.UseCases.Auth
{
    public sealed class LoginUserUseCase
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenServices _tokenServices;
        private readonly IValidator<LoginRequest> _validator;
        private readonly ILogger<LoginUserUseCase> _logger;

        public LoginUserUseCase(IIdentityService identityService, ITokenServices tokenServices, IValidator<LoginRequest> validator, ILogger<LoginUserUseCase> logger)
        {
            _identityService = identityService;
            _tokenServices = tokenServices;
            _validator = validator;
            _logger = logger;
        }

        public async Task<LoginUserResponse> GetTokenAsync(
            LoginRequest request,
            CancellationToken cancellation)
        {
            _logger.LogInformation(
                "LoginUser started. Email={Email}",
                request.Email);

            var validationResult = await _validator.ValidateAsync(request, cancellation);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "LoginUser validation failed. Email={Email}, Errors={Errors}",
                    request.Email,
                    validationResult.Errors.Select(e => e.ErrorMessage));

                throw new ValidationException(validationResult.Errors);
            }

            _logger.LogInformation(
                "Creating identity user. Email={Email}",
                request.Email);

            var user = await _identityService.LoginUserAsync(
                request.Email,
                request.Password,
                cancellation);

            _logger.LogInformation(
                "Retrieve user successfully. UserId={UserId}",
                user.Id);

            var token = await _tokenServices.CreateTokenAsync(user);

            _logger.LogInformation(
                "Token generated successfully. UserId={UserId}",
                user.Id);

            _logger.LogInformation(
                "GetUser finished successfully. Email={Email}",
                request.Email);

            return new LoginUserResponse(token);
        }
    }
}

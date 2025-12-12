using FluentValidation;
using Fundo.Aplications.Aplication.Interfaces.Services;
using Fundo.Applications.Domain.Requests.Auth;
using Fundo.Applications.Domain.Responses.Auth;
using Microsoft.Extensions.Logging;

namespace Fundo.Aplications.Aplication.UseCases.Auth
{
    public sealed class CreateUserUseCase
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenServices _tokenServices;
        private readonly IValidator<CreateUserRequest> _validator;
        private readonly ILogger<CreateUserUseCase> _logger;

        public CreateUserUseCase(
            IIdentityService identityService,
            ITokenServices tokenServices,
            IValidator<CreateUserRequest> validator,
            ILogger<CreateUserUseCase> logger)
        {
            _identityService = identityService;
            _tokenServices = tokenServices;
            _validator = validator;
            _logger = logger;
        }

        public async Task<CreateUserResponse> CreateUserAsync(
            CreateUserRequest request,
            CancellationToken cancellation)
        {
            _logger.LogInformation(
                "CreateUser started. Email={Email}",
                request.Email);

            var validationResult = await _validator.ValidateAsync(request, cancellation);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "CreateUser validation failed. Email={Email}, Errors={Errors}",
                    request.Email,
                    validationResult.Errors.Select(e => e.ErrorMessage));

                throw new ValidationException(validationResult.Errors);
            }

            _logger.LogInformation(
                "Creating identity user. Email={Email}",
                request.Email);

            var user = await _identityService.CreateUserAsync(
                request.Email,
                request.Password,
                cancellation);

            _logger.LogInformation(
                "User created successfully. UserId={UserId}",
                user.Id);

            var token = await _tokenServices.CreateTokenAsync(user);

            _logger.LogInformation(
                "Token generated successfully. UserId={UserId}",
                user.Id);

            _logger.LogInformation(
                "CreateUser finished successfully. Email={Email}",
                request.Email);

            return new CreateUserResponse(token);
        }
    }
}

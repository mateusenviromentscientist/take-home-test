using FluentValidation;
using FluentValidation.Results;
using Fundo.Aplications.Aplication.Interfaces.Services;
using Fundo.Aplications.Aplication.UseCases.Auth;
using Fundo.Applications.Domain.Dtos;
using Fundo.Applications.Domain.Requests.Auth;
using Fundo.Applications.Domain.Responses.Auth;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.Auth
{
    public sealed class LoginUserUseCaseTests
    {
        private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
        private readonly ITokenServices _tokenServices = Substitute.For<ITokenServices>();
        private readonly IValidator<LoginRequest> _validator = Substitute.For<IValidator<LoginRequest>>();
        private readonly ILogger<LoginUserUseCase> _logger = Substitute.For<ILogger<LoginUserUseCase>>();

        private LoginUserUseCase CreateSut()
            => new(_identityService, _tokenServices, _validator, _logger);

        [Fact]
        public async Task GetTokenAsync_WhenValidationFails_ShouldThrowValidationException_AndNotCallServices()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new LoginRequest("user@test.com", "123");

            var failures = new[]
            {
                new ValidationFailure(nameof(LoginRequest.Email), "Invalid email")
            };

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult(failures));

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.GetTokenAsync(request, ct));

            await _identityService.DidNotReceive()
                .LoginUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

            await _tokenServices.DidNotReceive()
                .CreateTokenAsync(Arg.Any<AuthenticatedUser>());
        }

        [Fact]
        public async Task GetTokenAsync_WhenValidationPasses_ShouldLoginUser_AndGenerateToken_AndReturnResponse()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new LoginRequest("user@test.com", "123");

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            var user = new AuthenticatedUser("user-1", request.Email, null);

            _identityService
                .LoginUserAsync(request.Email, request.Password, ct)
                .Returns(user);

            _tokenServices
                .CreateTokenAsync(user)
                .Returns("jwt-token");

            // Act
            LoginUserResponse response = await sut.GetTokenAsync(request, ct);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("jwt-token", response.Token);

            await _identityService.Received(1)
                .LoginUserAsync(request.Email, request.Password, ct);

            await _tokenServices.Received(1)
                .CreateTokenAsync(user);
        }

        [Fact]
        public async Task GetTokenAsync_ShouldCallLoginBeforeTokenGeneration()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new LoginRequest("user@test.com", "123");

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            var user = new AuthenticatedUser("user-99", request.Email, null);

            _identityService
                .LoginUserAsync(Arg.Any<string>(), Arg.Any<string>(), ct)
                .Returns(user);

            _tokenServices
                .CreateTokenAsync(user)
                .Returns("token-order");

            // Act
            await sut.GetTokenAsync(request, ct);

            // Assert
            Received.InOrder(() =>
            {
                _identityService.LoginUserAsync(request.Email, request.Password, ct);
                _tokenServices.CreateTokenAsync(user);
            });
        }
    }
}

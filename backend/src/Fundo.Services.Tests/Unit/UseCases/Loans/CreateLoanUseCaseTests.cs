using FluentValidation;
using FluentValidation.Results;
using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Aplications.Aplication.UseCases.Loans;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Requests.Loans;
using Fundo.Applications.Domain.Responses.Loans;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.Loans
{
    public sealed class CreateLoanUseCaseTests
    {
        private readonly ILoanRepository _loanRepository = Substitute.For<ILoanRepository>();
        private readonly ILogger<CreateLoanUseCase> _logger = Substitute.For<ILogger<CreateLoanUseCase>>();
        private readonly IValidator<CreateLoanRequest> _validator = Substitute.For<IValidator<CreateLoanRequest>>();

        private CreateLoanUseCase CreateSut()
            => new(_loanRepository, _logger, _validator);

        [Fact]
        public async Task CreateLoanAsync_WhenValidationFails_ShouldThrowValidationException_AndNotCallRepository()
        {
            // Arrange
            var sut = CreateSut();
            var request = new CreateLoanRequest(1000, 1000, "Teste");
            

            var failures = new[]
            {
                new ValidationFailure(nameof(CreateLoanRequest.Amount), "Amount must be greater than zero")
            };

            _validator
                .ValidateAsync(request, Arg.Any<CancellationToken>())
                .Returns(new ValidationResult(failures));

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.CreateLoanAsync(request, CancellationToken.None));

            await _loanRepository
                .DidNotReceive()
                .CreateLoanAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateLoanAsync_WhenValidationPasses_AndRepositoryReturnsFalse_ShouldReturnResponseFalse()
        {
            // Arrange
            var sut = CreateSut();
            var request = new CreateLoanRequest(1000, 1000, "Teste");

            _validator
                .ValidateAsync(request, Arg.Any<CancellationToken>())
                .Returns(new ValidationResult()); // IsValid = true

            _loanRepository
                .CreateLoanAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            CreateLoanResponse response = await sut.CreateLoanAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);

            await _loanRepository
                .Received(1)
                .CreateLoanAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateLoanAsync_WhenValidationPasses_AndRepositoryReturnsTrue_ShouldReturnResponseTrue()
        {
            // Arrange
            var sut = CreateSut();
            var request = new CreateLoanRequest(1000, 1000, "Teste");

            _validator
                .ValidateAsync(request, Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            _loanRepository
                .CreateLoanAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            CreateLoanResponse response = await sut.CreateLoanAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success); 
        }

        [Fact]
        public async Task CreateLoanAsync_ShouldMapRequestToLoanModelCorrectly()
        {
            // Arrange
            var sut = CreateSut();
            var request = new CreateLoanRequest(1000, 1000, "Teste");

            _validator
                .ValidateAsync(request, Arg.Any<CancellationToken>())
                .Returns(new ValidationResult()); 

            _loanRepository
                .CreateLoanAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>())
                .Returns(true);

            LoanModel? captured = null;

            _loanRepository
                .When(x => x.CreateLoanAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>()))
                .Do(ci => captured = ci.ArgAt<LoanModel>(0));

            // Act
            await sut.CreateLoanAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(request.Amount, captured!.Amount);
            Assert.Equal(request.ApplicationName, captured.ApplicantName);
            Assert.Equal(request.Amount, captured.CurrentBalance);
            Assert.Equal(Applications.Domain.Enums.LoanStatus.Active, captured.Status);
        }
    }
}

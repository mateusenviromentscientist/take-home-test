using FluentValidation;
using FluentValidation.Results;
using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Aplications.Aplication.UseCases.Loans;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Requests.Loans;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.Loans
{
    public sealed class PayLoanUseCaseTests
    {
        private readonly ILoanRepository _loanRepository = Substitute.For<ILoanRepository>();
        private readonly IValidator<PayLoanRequest> _validator = Substitute.For<IValidator<PayLoanRequest>>();
        private readonly ILogger<PayLoanUseCase> _logger = Substitute.For<ILogger<PayLoanUseCase>>();

        private PayLoanUseCase CreateSut()
            => new(_loanRepository, _validator, _logger);

        [Fact]
        public async Task PayLoanAsync_WhenValidationFails_ShouldThrowValidationException_AndNotCallRepository()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new PayLoanRequest(1, 30m);

            var failures = new[]
            {
                new ValidationFailure(nameof(PayLoanRequest.Amount), "Amount must be greater than zero")
            };

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult(failures));

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => sut.PayLoanAsync(request, ct));

            await _loanRepository.DidNotReceive()
                .GetLoanModelByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

            await _loanRepository.DidNotReceive()
                .UpdateLoanPaymentAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PayLoanAsync_WhenLoanNotFound_ShouldReturnFalse_AndNotUpdate()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new PayLoanRequest(1, 30m);

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            _loanRepository
                .GetLoanModelByIdAsync(request.Id, ct)
                .Returns((LoanModel?)null);

            // Act
            var response = await sut.PayLoanAsync(request, ct);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task PayLoanAsync_WhenAmountGreaterThanCurrentBalance_ShouldReturnFalse_AndNotUpdate()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new PayLoanRequest(1, 30m);

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            var loan = new LoanModel
            {
                CurrentBalance = 100m,
                Status = Applications.Domain.Enums.LoanStatus.Active
            };

            _loanRepository
                .GetLoanModelByIdAsync(request.Id, ct)
                .Returns(loan);

            // Act
            var response = await sut.PayLoanAsync(request, ct);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);

            Assert.Equal(70m, loan.CurrentBalance);
            Assert.Equal(Applications.Domain.Enums.LoanStatus.Active, loan.Status);

            await _loanRepository.Received()
                .UpdateLoanPaymentAsync(Arg.Any<LoanModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PayLoanAsync_WhenPaymentIsValid_ButUpdateFails_ShouldReturnFalse()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new PayLoanRequest(1, 30m);

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            var loan = new LoanModel
            {
                CurrentBalance = 100m,
                Status = Applications.Domain.Enums.LoanStatus.Active
            };

            _loanRepository
                .GetLoanModelByIdAsync(1, ct)
                .Returns(loan);

            _loanRepository
                .UpdateLoanPaymentAsync(Arg.Any<LoanModel>(), ct)
                .Returns(false);

            // Act
            var response = await sut.PayLoanAsync(request, ct);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);

            Assert.Equal(70m, loan.CurrentBalance);

            await _loanRepository.Received(1)
                .UpdateLoanPaymentAsync(loan, ct);
        }

        [Fact]
        public async Task PayLoanAsync_WhenPaymentIsValid_AndUpdateSucceeds_ShouldReturnTrue_AndUpdateLoan()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new PayLoanRequest(1, 30m);

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            var loan = new LoanModel
            {
                CurrentBalance = 100m,
                Status = Applications.Domain.Enums.LoanStatus.Active
            };

            _loanRepository
                .GetLoanModelByIdAsync(1, ct)
                .Returns(loan);

            _loanRepository
                .UpdateLoanPaymentAsync(Arg.Any<LoanModel>(), ct)
                .Returns(true);

            // Act
            var response = await sut.PayLoanAsync(request, ct);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);

            Assert.Equal(70m, loan.CurrentBalance);
            Assert.Equal(Applications.Domain.Enums.LoanStatus.Active, loan.Status);

            await _loanRepository.Received(1)
                .UpdateLoanPaymentAsync(loan, ct);
        }

        [Fact]
        public async Task PayLoanAsync_WhenPaymentQuitsLoan_ShouldSetStatusToPaid()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new PayLoanRequest(1, 100m);

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            var loan = new LoanModel
            {
                CurrentBalance = 100m,
                Status = Applications.Domain.Enums.LoanStatus.Active
            };

            _loanRepository
                .GetLoanModelByIdAsync(1, ct)
                .Returns(loan);

            _loanRepository
                .UpdateLoanPaymentAsync(Arg.Any<LoanModel>(), ct)
                .Returns(true);

            // Act
            var response = await sut.PayLoanAsync(request, ct);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(0m, loan.CurrentBalance);
            Assert.Equal(Applications.Domain.Enums.LoanStatus.Paid, loan.Status);
        }
    }
}

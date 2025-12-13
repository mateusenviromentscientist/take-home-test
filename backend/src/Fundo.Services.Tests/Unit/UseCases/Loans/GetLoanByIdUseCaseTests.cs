using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Aplications.Aplication.UseCases.Loans;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Requests.Loans;
using Fundo.Applications.Domain.Responses.Loans;
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
    public sealed class GetLoanByIdUseCaseTests
    {
        private readonly ILoanRepository _loanRepository = Substitute.For<ILoanRepository>();
        private readonly ILogger<GetLoanByIdUseCase> _logger = Substitute.For<ILogger<GetLoanByIdUseCase>>();
        private readonly IValidator<GetLoanByIdRequest> _validator = Substitute.For<IValidator<GetLoanByIdRequest>>();

        private GetLoanByIdUseCase CreateSut()
            => new(_loanRepository, _logger, _validator);

        [Fact]
        public async Task GetLoanByIdAsync_WhenValidationFails_ShouldThrowValidationException_AndNotCallRepository()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new GetLoanByIdRequest(10);

            var failures = new[]
            {
                new ValidationFailure(nameof(GetLoanByIdRequest.Id), "Id must be greater than 0")
            };

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult(failures));

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.GetLoanByIdAsync(request, ct));

            await _loanRepository
                .DidNotReceive()
                .GetLoanModelByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetLoanByIdAsync_WhenValidationPasses_AndRepositoryReturnsNull_ShouldReturnResponseWithNewLoanModel()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new GetLoanByIdRequest(10);

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            _loanRepository
                .GetLoanModelByIdAsync(request.Id, ct)
                .Returns((LoanModel?)null);

            // Act
            GetLoanByIdResponse response = await sut.GetLoanByIdAsync(request, ct);

            // Assert
            Assert.NotNull(response);

            Assert.NotNull(response);

            Assert.IsType<GetLoanByIdResponse>(response);

            await _loanRepository
                .Received(1)
                .GetLoanModelByIdAsync(request.Id, ct);
        }

        [Fact]
        public async Task GetLoanByIdAsync_WhenValidationPasses_AndRepositoryReturnsLoan_ShouldReturnLoanInResponse()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var request = new GetLoanByIdRequest(7);

            _validator
                .ValidateAsync(request, ct)
                .Returns(new ValidationResult());

            var loan = new LoanModel
            {
                Amount = 500m,
                ApplicantName = "Mateus",
                CurrentBalance = 500m,
                Status = Applications.Domain.Enums.LoanStatus.Active,
                Id = 7
            };

            _loanRepository
                .GetLoanModelByIdAsync(request.Id, ct)
                .Returns(loan);

            // Act
            GetLoanByIdResponse response = await sut.GetLoanByIdAsync(request, ct);

            // Assert
            Assert.NotNull(response);

            await _loanRepository
                .Received(1)
                .GetLoanModelByIdAsync(request.Id, ct);
        }
    }
}

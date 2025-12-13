using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Aplications.Aplication.UseCases.Loans;
using Fundo.Applications.Domain.Entities;
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
    public sealed class GetAllLoansUseCaseTests
    {
        private readonly ILoanRepository _loanRepository = Substitute.For<ILoanRepository>();
        private readonly ILogger<GetAllLoansUseCase> _logger = Substitute.For<ILogger<GetAllLoansUseCase>>();

        private GetAllLoansUseCase CreateSut()
            => new(_loanRepository, _logger);

        [Fact]
        public async Task GetAllLoansAsync_WhenRepositoryReturnsEmptyList_ShouldReturnEmptyResponse()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            _loanRepository
                .GetAllLoansAsync(ct)
                .Returns(new List<LoanModel>()); // vazio

            // Act
            GetAllLoansResponse response = await sut.GetAllLoansAsync(ct);

            // Assert
            Assert.NotNull(response);

            Assert.NotNull(response.LoanModels);
            Assert.Empty(response.LoanModels);

            await _loanRepository
                .Received(1)
                .GetAllLoansAsync(ct);
        }

        [Fact]
        public async Task GetAllLoansAsync_WhenRepositoryReturnsLoans_ShouldReturnLoansInResponse()
        {
            // Arrange
            var sut = CreateSut();
            var ct = CancellationToken.None;

            var loans = new List<LoanModel>
            {
                new LoanModel { Amount = 100m, ApplicantName = "Ana" },
                new LoanModel { Amount = 200m, ApplicantName = "Carlos" }
            };

            _loanRepository
                .GetAllLoansAsync(ct)
                .Returns(loans);

            // Act
            GetAllLoansResponse response = await sut.GetAllLoansAsync(ct);

            // Assert
            Assert.NotNull(response);

            Assert.NotNull(response);
            Assert.Equal(2, response.LoanModels.Count);

            Assert.Same(loans, response.LoanModels);

            await _loanRepository
                .Received(1)
                .GetAllLoansAsync(ct);
        }
    }
}

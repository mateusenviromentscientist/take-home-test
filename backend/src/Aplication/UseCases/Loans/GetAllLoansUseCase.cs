using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Responses.Loans;
using Microsoft.Extensions.Logging;

namespace Fundo.Aplications.Aplication.UseCases.Loans
{
    public sealed class GetAllLoansUseCase
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<GetAllLoansUseCase> _logger;

        public GetAllLoansUseCase(ILoanRepository loanRepository, ILogger<GetAllLoansUseCase> logger)
        {
            _loanRepository = loanRepository;
            _logger = logger;
        }

        public async Task<GetAllLoansResponse> GetAllLoansAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "GetAllLoans started");

            var loans = await _loanRepository.GetAllLoansAsync(cancellationToken);

            if (!loans.Any())
            {
                _logger.LogWarning("Empty Loans");
                return new GetAllLoansResponse(new List<LoanModel>());
            }

            _logger.LogInformation(
               "GetAllLoans retrieve with success and contais {Loans.Count}", loans.Count);

            return new GetAllLoansResponse(loans);
        }
    }
}

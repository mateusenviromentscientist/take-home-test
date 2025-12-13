using Fundo.Applications.Domain.Entities;

namespace Fundo.Aplications.Aplication.Interfaces.Repositories
{
    public interface ILoanRepository
    {
        Task<IReadOnlyList<LoanModel>> GetAllLoansAsync(CancellationToken cancellationToken);

        Task<LoanModel?> GetLoanModelByIdAsync(int id, CancellationToken cancellationToken);

        Task<bool> CreateLoanAsync(LoanModel model, CancellationToken cancellationToken);

        Task<bool> UpdateLoanPaymentAsync(LoanModel model, CancellationToken cancellationToken);
    }
}

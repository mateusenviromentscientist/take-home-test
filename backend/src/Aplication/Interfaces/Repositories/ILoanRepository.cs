using Fundo.Applications.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Aplications.Aplication.Interfaces.Repositories
{
    public interface ILoanRepository
    {
        Task<IReadOnlyList<LoanModel>> GetAllLoans(CancellationToken cancellationToken);

        Task<LoanModel> GetLoanModelById(int id, CancellationToken cancellationToken);

        Task<bool> CreateLoan(LoanModel model, CancellationToken cancellationToken);

        Task<bool> UpdateLoanPayment(int id, LoanModel model, CancellationToken cancellationToken);
    }
}

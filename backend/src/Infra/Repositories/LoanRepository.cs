using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Applications.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.Infra.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        public Task<bool> CreateLoan(LoanModel model, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<LoanModel>> GetAllLoans(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<LoanModel> GetLoanModelById(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateLoanPayment(int id, LoanModel model, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

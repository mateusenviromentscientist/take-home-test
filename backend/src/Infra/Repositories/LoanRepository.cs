using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.Infra.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _appContext;

        public LoanRepository(AppDbContext appContext)
        {
            _appContext = appContext;
        }

        public async Task<bool> CreateLoanAsync(LoanModel model, CancellationToken cancellationToken)
        {
            await _appContext.AddAsync(model, cancellationToken);

            await _appContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        
        public async Task<IReadOnlyList<LoanModel>> GetAllLoansAsync(CancellationToken cancellationToken) =>
            await _appContext.Loans.AsNoTracking().ToListAsync(cancellationToken);


        public async Task<LoanModel?> GetLoanModelByIdAsync(int id, CancellationToken cancellationToken) =>
            await _appContext.Loans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        

        public async Task<bool> UpdateLoanPaymentAsync(LoanModel model, CancellationToken cancellationToken)
        {
            _appContext.Update(model);

            await _appContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

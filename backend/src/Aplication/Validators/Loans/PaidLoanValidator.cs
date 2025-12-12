using FluentValidation;
using Fundo.Applications.Domain.Requests.Loans;

namespace Fundo.Aplications.Aplication.Validators.Loans
{
    public sealed class PaidLoanValidator : AbstractValidator<PaidLoanRequest>
    {
        public PaidLoanValidator()
        {
            RuleFor(x => x.Id).NotNull();
            RuleFor(x => x.Amount).NotNull().GreaterThan(0);
        }
    }
}

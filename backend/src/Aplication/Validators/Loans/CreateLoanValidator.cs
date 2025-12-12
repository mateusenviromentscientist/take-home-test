using FluentValidation;
using Fundo.Applications.Domain.Requests.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Aplications.Aplication.Validators.Loans
{
    public sealed class CreateLoanValidator : AbstractValidator<CreateLoanRequest>
    {
        public CreateLoanValidator()
        {
            RuleFor(x => x.CurrentBalance).NotNull().GreaterThan(0);
            RuleFor(x => x.ApplicationName).NotEmpty();
            RuleFor(x => x.Amount).NotNull().GreaterThan(0);
        }
    }
}

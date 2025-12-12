using FluentValidation;
using Fundo.Applications.Domain.Requests.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Aplications.Aplication.Validators.Loans
{
    public sealed class GetLoanByIdValidator : AbstractValidator<GetLoanByIdRequest>
    {
        public GetLoanByIdValidator()
        {
            RuleFor(x => x.Id).NotNull();
        }
    }
}

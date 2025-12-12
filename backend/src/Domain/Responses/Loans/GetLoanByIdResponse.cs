using Fundo.Applications.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.Domain.Responses.Loans
{
    public record class GetLoanByIdResponse(LoanModel Model);
}

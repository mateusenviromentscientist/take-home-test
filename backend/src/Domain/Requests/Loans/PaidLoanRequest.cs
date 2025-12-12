using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.Domain.Requests.Loans
{
    public record class PaidLoanRequest(int Id, decimal Amount);
}

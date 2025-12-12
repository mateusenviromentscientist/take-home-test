using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.Domain.Requests.Loans
{
    public record class CreateLoanRequest(decimal Amount, decimal CurrentBalance, string ApplicationName);
}

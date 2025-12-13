using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.Domain.Requests.Auth
{
    public sealed record class CreateUserRequest (string Email, string Password);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.Domain.Dtos
{
    public record class AuthenticatedUser(string Id, string Email, IReadOnlyList<string?> Roles);
}

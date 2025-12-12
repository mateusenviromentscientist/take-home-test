using Fundo.Applications.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Aplications.Aplication.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<AuthenticatedUser> CreateUserAsync(string email, string password, CancellationToken cancellationToken);
        Task<AuthenticatedUser> LoginUserAsync(string email, string password, CancellationToken cancellationToken);
    }
}

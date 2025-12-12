using Fundo.Applications.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Aplications.Aplication.Interfaces.Services
{
    public interface ITokenServices
    {
        Task<string> CreateTokenAsync(AuthenticatedUser user);
    }
}

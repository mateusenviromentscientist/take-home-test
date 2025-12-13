using Fundo.Aplications.Aplication.UseCases.Auth;
using Fundo.Applications.Domain.Requests.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly CreateUserUseCase _createUserUseCase;
        private readonly LoginUserUseCase _loginUserUseCase;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            CreateUserUseCase createUserUseCase,
            LoginUserUseCase loginUserUseCase,
            ILogger<AuthController> logger)
        {
            _createUserUseCase = createUserUseCase;
            _loginUserUseCase = loginUserUseCase;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {

            _logger.LogInformation("Auth register request received. Email={Email}", request.Email);

            var response = await _createUserUseCase.CreateUserAsync(request, cancellationToken);

            return StatusCode(201, response);
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auth login request received. Email={Email}", request.Email);

            var response = await _loginUserUseCase.GetTokenAsync(request, cancellationToken);

            return Ok(response);
        }


        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirst("sub")?.Value
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            });
        }
    }
}

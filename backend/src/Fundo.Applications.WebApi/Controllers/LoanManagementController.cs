using Fundo.Aplications.Aplication.UseCases.Loans;
using Fundo.Applications.Domain.Requests.Loans;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("loans")]
    public sealed class LoanManagementController : ControllerBase
    {
        private readonly CreateLoanUseCase _createLoanUseCase;
        private readonly GetLoanByIdUseCase _getLoanByIdUseCase;
        private readonly GetAllLoansUseCase _getAllLoansUseCase;
        private readonly PayLoanUseCase _payLoanUseCase;
        private readonly ILogger<LoanManagementController> _logger;

        public LoanManagementController(
            CreateLoanUseCase createLoanUseCase,
            GetLoanByIdUseCase getLoanByIdUseCase,
            GetAllLoansUseCase getAllLoansUseCase,
            PayLoanUseCase payLoanUseCase,
            ILogger<LoanManagementController> logger)
        {
            _createLoanUseCase = createLoanUseCase;
            _getLoanByIdUseCase = getLoanByIdUseCase;
            _getAllLoansUseCase = getAllLoansUseCase;
            _payLoanUseCase = payLoanUseCase;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLoans(CancellationToken cancellationToken)
            => Ok(await _getAllLoansUseCase.GetAllLoansAsync(cancellationToken));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLoanById([FromRoute] int id, CancellationToken cancellationToken)
        {
            var request = new GetLoanByIdRequest(id);
            _logger.LogInformation("GetLoanById request received. Id={Id}", id);

            var response = await _getLoanByIdUseCase.GetLoanByIdAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("CreateLoan request received");

            var result = await _createLoanUseCase.CreateLoanAsync(request, cancellationToken);

            return CreatedAtRoute("loans", result);
        }

        [HttpPost("{id:int}/payment")]
        public async Task<IActionResult> LoanPayment([FromRoute] int id, [FromBody] PayLoanRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PayLoan request received. Id={Id}", id);

            var result = await _payLoanUseCase.PayLoanAsync(request, cancellationToken);

            return CreatedAtRoute("{id:int}/payment", result);
        }
    }
}
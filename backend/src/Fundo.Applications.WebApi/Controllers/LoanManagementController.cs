using Fundo.Aplications.Aplication.UseCases.Loans;
using Fundo.Applications.Domain.Requests.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("loan")]
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
        {
            var loans = await _getAllLoansUseCase.GetAllLoansAsync(cancellationToken);
            return Ok(loans);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLoanById([FromRoute] int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetLoanById request received. Id={Id}", id);

            var request = new GetLoanByIdRequest(id);
            var loan = await _getLoanByIdUseCase.GetLoanByIdAsync(request, cancellationToken);

            if (loan is null)
                return NotFound();

            return Ok(loan);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan(
            [FromBody] CreateLoanRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("CreateLoan request received");

            var success = await _createLoanUseCase.CreateLoanAsync(request, cancellationToken);
            return Ok(success);
        }

        [HttpPost("{id:int}/payment")]
        public async Task<IActionResult> LoanPayment(
            [FromRoute] int id,
            [FromBody] PayLoanRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("PayLoan request received. Id={Id}", id);

            var success = await _payLoanUseCase.PayLoanAsync(request, cancellationToken);
            return Ok(success);
        }
    }
}

using FluentValidation;
using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Requests.Loans;
using Fundo.Applications.Domain.Responses.Loans;
using Microsoft.Extensions.Logging;

namespace Fundo.Aplications.Aplication.UseCases.Loans
{
    public sealed class PayLoanUseCase
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IValidator<PayLoanRequest> _validator;
        private readonly ILogger<PayLoanUseCase> _logger;

        public PayLoanUseCase(ILoanRepository loanRepository, IValidator<PayLoanRequest> validator, ILogger<PayLoanUseCase> logger)
        {
            _loanRepository = loanRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<PayLoanResponse> PayLoanAsync(PayLoanRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "PayLoan started");

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("PayLoan validation failed.");

                throw new ValidationException(validationResult.Errors);
            }

            _logger.LogInformation(
               "PayLoan starting");

            var getLoanById = await _loanRepository.GetLoanModelByIdAsync(request.Id, cancellationToken);

            if(getLoanById is null)
            {
                _logger.LogWarning("Loan with Id = {Id} not found", request.Id);

                return new PayLoanResponse(false);
            }

            var (success, message) = MakePayment(request, getLoanById);

            if (!success)
            {
                _logger.LogWarning(message);

                return new PayLoanResponse(false);
            }

            var result =  await _loanRepository.UpdateLoanPaymentAsync(getLoanById, cancellationToken);

            if (!result)
            {
                _logger.LogWarning("fail when try to make the payment");
                return new PayLoanResponse(false);
            }

            return new PayLoanResponse(result);
        }

        private static (bool success, string? message) MakePayment(PayLoanRequest request, LoanModel model)
        {
            if(request.Amount > model.CurrentBalance)
            {
                return (false, "Amount should be not greather than current Balance");
            }

            model.CurrentBalance = model.CurrentBalance - request.Amount;

            if (model.CurrentBalance == 0)
                model.Status = Applications.Domain.Enums.LoanStatus.Paid;

            return (true, null);
        }
    }
}

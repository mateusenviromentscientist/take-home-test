using FluentValidation;
using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Requests.Loans;
using Fundo.Applications.Domain.Responses.Loans;
using Microsoft.Extensions.Logging;

namespace Fundo.Aplications.Aplication.UseCases.Loans
{
    public sealed class CreateLoanUseCase
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<CreateLoanUseCase> _logger;
        private readonly IValidator<CreateLoanRequest> _validator;

        public CreateLoanUseCase(ILoanRepository loanRepository, ILogger<CreateLoanUseCase> logger, IValidator<CreateLoanRequest> validator)
        {
            _loanRepository = loanRepository;
            _logger = logger;
            _validator = validator;
        }

        public async Task<CreateLoanResponse> CreateLoanAsync(CreateLoanRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "CreateLoan started");

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("CreateLoan validation failed.");

                throw new ValidationException(validationResult.Errors);
            }

            _logger.LogInformation(
                "CreateLoan starting");

            var loan = await _loanRepository.CreateLoanAsync(CreateModel(request), cancellationToken);

            if (!loan)
            {
                _logger.LogWarning("CreateLoan Failed");
                return new CreateLoanResponse(false);
            }

            _logger.LogInformation("CreateLoan finished successfully.");

            return new CreateLoanResponse(loan);
        }

        private static LoanModel CreateModel(CreateLoanRequest request) =>
            new()
            {
                Amount = request.Amount,
                ApplicantName = request.ApplicationName,
                CurrentBalance = request.Amount,
                Status = Applications.Domain.Enums.LoanStatus.Active,
            };
    }
}

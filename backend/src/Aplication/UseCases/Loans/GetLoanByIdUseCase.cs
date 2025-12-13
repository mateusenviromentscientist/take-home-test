using FluentValidation;
using Fundo.Aplications.Aplication.Interfaces.Repositories;
using Fundo.Applications.Domain.Requests.Loans;
using Fundo.Applications.Domain.Responses.Loans;
using Microsoft.Extensions.Logging;

namespace Fundo.Aplications.Aplication.UseCases.Loans
{
    public sealed class GetLoanByIdUseCase
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<GetLoanByIdUseCase> _logger;
        private readonly IValidator<GetLoanByIdRequest> _validator;

        public GetLoanByIdUseCase(ILoanRepository loanRepository, ILogger<GetLoanByIdUseCase> logger, IValidator<GetLoanByIdRequest> validator)
        {
            _loanRepository = loanRepository;
            _logger = logger;
            _validator = validator;
        }

        public async Task<GetLoanByIdResponse> GetLoanByIdAsync(GetLoanByIdRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "GetLoanById started");

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("GetLoanById validation failed.");

                throw new ValidationException(validationResult.Errors);
            }

            _logger.LogInformation("GetLoanById started");

            var loanById = await _loanRepository.GetLoanModelByIdAsync(request.Id, cancellationToken);

            if(loanById is null)
            {
                _logger.LogWarning("GetLoanById not found for Id. Id={Id}", request.Id);
                return new GetLoanByIdResponse(new Applications.Domain.Entities.LoanModel());
            }

            _logger.LogInformation(
                "GetLoanById retrieve with success for Id={Id}",
                request.Id);

            return new GetLoanByIdResponse(loanById);
        }
    }
}

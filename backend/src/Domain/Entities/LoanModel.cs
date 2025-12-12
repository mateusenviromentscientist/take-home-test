using Fundo.Applications.Domain.Enums;

namespace Fundo.Applications.Domain.Entities
{
    public sealed class LoanModel
    {
        public int Id { get; set; }

        public decimal? CurrentBalance { get; set; }

        public decimal? Amount { get; set; }

        public string? ApplicantName { get; set; }

        public LoanStatus? Status { get; set; }
    }
}

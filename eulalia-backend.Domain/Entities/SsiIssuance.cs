using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using eulalia_backend.Domain.Enums;

namespace eulalia_backend.Domain.Entities
{
    [Table("ssi_issuances")]
    public class SsiIssuance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Cedula { get; set; } = default!;

        public string? HolderDid { get; set; }
        public string? IssuerDid { get; set; }
        public string? SchemaId { get; set; }
        public string? InvitationId { get; set; }
        public string? InvitationUrl { get; set; }
        public string? CredentialRecordId { get; set; }

        [Required]
        public SsiIssuanceStatus Status { get; set; } = SsiIssuanceStatus.Requested;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? ErrorMessage { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eulalia_backend.Domain.Entities;

[Table("organizacion_identidad")]
public class OrganizacionIdentidad
{
    [Key]
    public int Id { get; set; }
    public int OrganizacionId { get; set; }
    public string? ClientDid { get; set; }
    public string? HolderConnectionId { get; set; }
    public string? LongFormDid { get; set; }
    public string? PublishedDid { get; set; }
    public string Estado { get; set; } = "pending";
    public string? PublicationOperationId { get; set; }
    public string? IssuerReference { get; set; }
    public string? CredentialRecordId { get; set; }
    public string? CredentialStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }

    [ForeignKey(nameof(OrganizacionId))]
    public Organizacion Organizacion { get; set; } = default!;
}

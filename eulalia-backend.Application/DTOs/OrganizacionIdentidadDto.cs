namespace eulalia_backend.Application.DTOs;

public class OrganizacionIdentidadDto
{
    public int OrganizacionId { get; set; }
    public string Nombre { get; set; } = default!;
    public string EstadoAprobacion { get; set; } = default!;
    public string? Did { get; set; }
    public string? ClientDid { get; set; }
    public string? HolderConnectionId { get; set; }
    public string? LongFormDid { get; set; }
    public string? PublishedDid { get; set; }
    public string? EstadoIdentidad { get; set; }
    public string? EstadoCredencial { get; set; }
    public string? ReferenciaEmisor { get; set; }
    public string? ReferenciaOperacion { get; set; }
    public string? CredentialRecordId { get; set; }
    public DateTime? CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public string? Error { get; set; }
}

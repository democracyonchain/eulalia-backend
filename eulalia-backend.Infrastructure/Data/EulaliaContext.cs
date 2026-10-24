using eulalia_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eulalia_backend.Infrastructure.Data
{
    public class EulaliaContext: DbContext
    {
        public EulaliaContext(DbContextOptions<EulaliaContext> options) : base(options) { }

        public DbSet<Ciudadano> Ciudadanos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RolUsuario> Roles { get; set; }
        public DbSet<Organizacion> Organizaciones { get; set; }
        public DbSet<Afiliacion> Afiliaciones { get; set; }
        public DbSet<Blockchain> Blockchain { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<SolicitudOrganizacion> SolicitudOrganizacion { get; set; }
        public DbSet<SsiIssuance> SsiIssuances { get; set; }
        public DbSet<Parametrosistema> Parametrosistema { get; set; }

        public DbSet<BiometriaCiudadano> BiometriasCiudadano { get; set; }
        public DbSet<OrganizacionIdentidad> OrganizacionIdentidades { get; set; }
    


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación 1:1 entre Usuario y Ciudadano
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Ciudadano)
                .WithOne()
                .HasForeignKey<Usuario>(u => u.Cedula_Ciudadano)
                .HasPrincipalKey<Ciudadano>(c => c.Cedula)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<BiometriaCiudadano>(entity =>
            {
                entity.HasOne(b => b.Ciudadano)
                      .WithMany()
                      .HasForeignKey(b => b.Cedula)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BiometriaCiudadano>().ToTable("biometriaciudadano");

            // Keep compatibility with the existing development database schema,
            // whose legacy columns do not use snake_case separators.
            modelBuilder.Entity<Afiliacion>(entity =>
            {
                entity.Property(a => a.OrganizacionId).HasColumnName("organizacion_id");
                entity.Property(a => a.FechaAfiliacion).HasColumnName("fecha_afiliacion");
                entity.Property(a => a.MotivoCancelacion).HasColumnName("motivocancelacion");
                entity.Property(a => a.BlockchainTxId).HasColumnName("blockchain_tx_id");
                entity.Property(a => a.CardanoAffiliationEventId).HasColumnName("cardano_affiliation_event_id");
                entity.Property(a => a.CardanoAffiliationRecordHash).HasColumnName("cardano_affiliation_record_hash");
                entity.Property(a => a.CardanoAffiliationTxHash).HasColumnName("cardano_affiliation_tx_hash");
                entity.Property(a => a.CardanoAffiliationStatus).HasColumnName("cardano_affiliation_status");
                entity.Property(a => a.CardanoCancellationEventId).HasColumnName("cardano_cancellation_event_id");
                entity.Property(a => a.CardanoCancellationRecordHash).HasColumnName("cardano_cancellation_record_hash");
                entity.Property(a => a.CardanoCancellationTxHash).HasColumnName("cardano_cancellation_tx_hash");
                entity.Property(a => a.CardanoCancellationStatus).HasColumnName("cardano_cancellation_status");
                entity.Property(a => a.UsuarioAprobador).HasColumnName("usuarioaprobador");
                entity.Property(a => a.EsUltima).HasColumnName("esultima");

                entity.HasIndex(a => a.Cedula)
                    .HasDatabaseName("ux_afiliacion_cedula_active")
                    .IsUnique()
                    .HasFilter("estado <> 'Anulado'");
            });

            modelBuilder.Entity<OrganizacionIdentidad>(entity =>
            {
                entity.Property(i => i.ClientDid).HasColumnName("client_did");
                entity.Property(i => i.HolderConnectionId).HasColumnName("holder_connection_id");
                entity.HasIndex(i => i.OrganizacionId).IsUnique();
                entity.HasOne(i => i.Organizacion)
                    .WithMany()
                    .HasForeignKey(i => i.OrganizacionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }


    }

}

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
        }


    }

}

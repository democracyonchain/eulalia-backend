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
        public DbSet<SSI> SSIs { get; set; }
        public DbSet<SolicitudOrganizacion> Solicitudes { get; set; }
        public DbSet<Parametrosistema> Parametrosistema { get; set; }
        public DbSet<BiometriaCiudadano> BiometriasCiudadano { get; set; }
    


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Forzar nombres de tablas en minúsculas
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Convertir nombre de la tabla a minúsculas
                var tableName = entity.GetTableName();
                if (tableName != null && !tableName.StartsWith("__"))
                {
                    entity.SetTableName(tableName.ToLower());
                }

                // Convertir cada nombre de columna a minúsculas
                foreach (var property in entity.GetProperties())
                {
                    if (!entity.GetTableName()!.StartsWith("__"))
                        property.SetColumnName(property.Name.ToLower());
                }

                // Convertir claves primarias y foráneas
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName()!.ToLower());
                }

                foreach (var fk in entity.GetForeignKeys())
                {
                    fk.SetConstraintName(fk.GetConstraintName()!.ToLower());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName()!.ToLower());
                }
            }
            
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

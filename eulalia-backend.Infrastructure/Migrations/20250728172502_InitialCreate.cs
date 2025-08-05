using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eulalia_backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration  
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "afiliacion",
                columns: table => new
                {
                    afiliacion_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cedula = table.Column<string>(type: "text", nullable: false),
                    organizacionid = table.Column<int>(type: "integer", nullable: false),
                    fechaafiliacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    motivocancelacion = table.Column<string>(type: "text", nullable: true),
                    blockchaintxid = table.Column<int>(type: "integer", nullable: true),
                    usuarioaprobador = table.Column<int>(type: "integer", nullable: true),
                    esultima = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_afiliacion", x => x.afiliacion_id);
                });

            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    auditoriaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accion = table.Column<string>(type: "text", nullable: false),
                    usuarioid = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditoria", x => x.auditoriaid);
                });

            migrationBuilder.CreateTable(
                name: "blockchain",
                columns: table => new
                {
                    blockchainid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hashtransaccion = table.Column<string>(type: "text", nullable: false),
                    tipotransaccion = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "text", nullable: false),
                    fechatransaccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blockchain", x => x.blockchainid);
                });

            migrationBuilder.CreateTable(
                name: "ciudadano",
                columns: table => new
                {
                    cedula = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellido = table.Column<string>(type: "text", nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ciudadano", x => x.cedula);
                });

            migrationBuilder.CreateTable(
                name: "organizacion",
                columns: table => new
                {
                    organizacion_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    tipo_organizacion = table.Column<string>(type: "text", nullable: false),
                    codigo_provincia = table.Column<int>(type: "integer", nullable: true),
                    codigo_canton = table.Column<int>(type: "integer", nullable: true),
                    codigo_parroquia = table.Column<int>(type: "integer", nullable: true),
                    responsable_cedula = table.Column<string>(type: "text", nullable: false),
                    estado_validacion = table.Column<string>(type: "text", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizacion", x => x.organizacion_id);
                });

            migrationBuilder.CreateTable(
                name: "parametrosistema",
                columns: table => new
                {
                    parametroid = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    seccion = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parametrosistema", x => x.parametroid);
                });

            migrationBuilder.CreateTable(
                name: "provincia",
                columns: table => new
                {
                    codigo_provincia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provincia", x => x.codigo_provincia);
                });

            migrationBuilder.CreateTable(
                name: "rolusuario",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rolusuario", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "ssi",
                columns: table => new
                {
                    ssi_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cedula = table.Column<string>(type: "text", nullable: false),
                    credencial_digital = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_revocacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ssi", x => x.ssi_id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    correo = table.Column<string>(type: "text", nullable: false),
                    contrasena = table.Column<string>(type: "text", nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    cedula_ciudadano = table.Column<string>(type: "text", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.usuario_id);
                });

            migrationBuilder.CreateTable(
                name: "solicitudorganizacion",
                columns: table => new
                {
                    solicitud_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organizacion_id = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    fechasolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecharevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    usuariorevisor = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitudorganizacion", x => x.solicitud_id);
                    table.ForeignKey(
                        name: "fk_solicitudorganizacion_organizacion_organizacion_id",
                        column: x => x.organizacion_id,
                        principalTable: "organizacion",
                        principalColumn: "organizacion_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_solicitudorganizacion_organizacion_id",
                table: "solicitudorganizacion",
                column: "organizacion_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "afiliacion");

            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.DropTable(
                name: "blockchain");

            migrationBuilder.DropTable(
                name: "ciudadano");

            migrationBuilder.DropTable(
                name: "parametrosistema");

            migrationBuilder.DropTable(
                name: "provincia");

            migrationBuilder.DropTable(
                name: "rolusuario");

            migrationBuilder.DropTable(
                name: "solicitudorganizacion");

            migrationBuilder.DropTable(
                name: "ssi");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "organizacion");
        }
    }
}

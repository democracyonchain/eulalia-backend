using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eulalia_backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBiometriaCiudadano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "biometrias_ciudadano",
                columns: table => new
                {
                    biometriaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cedula = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    templatecifrado = table.Column<byte[]>(type: "bytea", nullable: false),
                    hashtemplate = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    fecharegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estadoverificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_biometrias_ciudadano", x => x.biometriaid);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organizacion_responsable_cedula",
                table: "organizacion",
                column: "responsable_cedula");

            migrationBuilder.AddForeignKey(
                name: "fk_organizacion_ciudadano_responsable_cedula",
                table: "organizacion",
                column: "responsable_cedula",
                principalTable: "ciudadano",
                principalColumn: "cedula",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organizacion_ciudadano_responsable_cedula",
                table: "organizacion");

            migrationBuilder.DropTable(
                name: "biometrias_ciudadano");

            migrationBuilder.DropIndex(
                name: "ix_organizacion_responsable_cedula",
                table: "organizacion");
        }
    }
}

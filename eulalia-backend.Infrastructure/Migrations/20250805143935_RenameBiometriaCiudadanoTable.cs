using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eulalia_backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameBiometriaCiudadanoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_biometria_ciudadano_ciudadano_cedula",
                table: "biometria_ciudadano");

            migrationBuilder.DropPrimaryKey(
                name: "pk_biometria_ciudadano",
                table: "biometria_ciudadano");

            migrationBuilder.RenameTable(
                name: "biometria_ciudadano",
                newName: "biometriaciudadano");

            migrationBuilder.RenameIndex(
                name: "ix_biometria_ciudadano_cedula",
                table: "biometriaciudadano",
                newName: "ix_biometrias_ciudadano_cedula");

            migrationBuilder.AddPrimaryKey(
                name: "pk_biometrias_ciudadano",
                table: "biometriaciudadano",
                column: "biometriaid");

            migrationBuilder.AddForeignKey(
                name: "fk_biometrias_ciudadano_ciudadano_cedula",
                table: "biometriaciudadano",
                column: "cedula",
                principalTable: "ciudadano",
                principalColumn: "cedula",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_biometrias_ciudadano_ciudadano_cedula",
                table: "biometriaciudadano");

            migrationBuilder.DropPrimaryKey(
                name: "pk_biometrias_ciudadano",
                table: "biometriaciudadano");

            migrationBuilder.RenameTable(
                name: "biometriaciudadano",
                newName: "biometria_ciudadano");

            migrationBuilder.RenameIndex(
                name: "ix_biometrias_ciudadano_cedula",
                table: "biometria_ciudadano",
                newName: "ix_biometria_ciudadano_cedula");

            migrationBuilder.AddPrimaryKey(
                name: "pk_biometria_ciudadano",
                table: "biometria_ciudadano",
                column: "biometriaid");

            migrationBuilder.AddForeignKey(
                name: "fk_biometria_ciudadano_ciudadano_cedula",
                table: "biometria_ciudadano",
                column: "cedula",
                principalTable: "ciudadano",
                principalColumn: "cedula",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

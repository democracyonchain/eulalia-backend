using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eulalia_backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBiometriaCiudadanoNamesAndFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_biometrias_ciudadano",
                table: "biometrias_ciudadano");

            migrationBuilder.RenameTable(
                name: "biometrias_ciudadano",
                newName: "biometria_ciudadano");

            migrationBuilder.AddPrimaryKey(
                name: "pk_biometria_ciudadano",
                table: "biometria_ciudadano",
                column: "biometriaid");

            migrationBuilder.CreateIndex(
                name: "ix_biometria_ciudadano_cedula",
                table: "biometria_ciudadano",
                column: "cedula");

            migrationBuilder.AddForeignKey(
                name: "fk_biometria_ciudadano_ciudadano_cedula",
                table: "biometria_ciudadano",
                column: "cedula",
                principalTable: "ciudadano",
                principalColumn: "cedula",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_biometria_ciudadano_ciudadano_cedula",
                table: "biometria_ciudadano");

            migrationBuilder.DropPrimaryKey(
                name: "pk_biometria_ciudadano",
                table: "biometria_ciudadano");

            migrationBuilder.DropIndex(
                name: "ix_biometria_ciudadano_cedula",
                table: "biometria_ciudadano");

            migrationBuilder.RenameTable(
                name: "biometria_ciudadano",
                newName: "biometrias_ciudadano");

            migrationBuilder.AddPrimaryKey(
                name: "pk_biometrias_ciudadano",
                table: "biometrias_ciudadano",
                column: "biometriaid");
        }
    }
}

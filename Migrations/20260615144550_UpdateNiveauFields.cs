using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionSalleEmploiTemps.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNiveauFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mention",
                table: "Niveaux",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Parcours",
                table: "Niveaux",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mention",
                table: "Niveaux");

            migrationBuilder.DropColumn(
                name: "Parcours",
                table: "Niveaux");
        }
    }
}

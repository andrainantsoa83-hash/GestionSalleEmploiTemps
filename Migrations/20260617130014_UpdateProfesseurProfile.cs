using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionSalleEmploiTemps.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfesseurProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adresse",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateNaissance",
                table: "Professeurs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRecrutement",
                table: "Professeurs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Departement",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DerniereConnexion",
                table: "Professeurs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diplome",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomaineSpecialisation",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstActif",
                table: "Professeurs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Fonction",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Matricule",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomUtilisateur",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sexe",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statut",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitreAcademique",
                table: "Professeurs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adresse",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "DateNaissance",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "DateRecrutement",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "Departement",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "DerniereConnexion",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "Diplome",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "DomaineSpecialisation",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "EstActif",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "Fonction",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "Matricule",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "NomUtilisateur",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "Sexe",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Professeurs");

            migrationBuilder.DropColumn(
                name: "TitreAcademique",
                table: "Professeurs");
        }
    }
}

using System;
using System.IO;
using System.Linq;
using GestionSalleEmploiTemps.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionSalleEmploiTemps.Services
{
    public interface IPdfGeneratorService
    {
        byte[] GenerateEmploiTempsPdf(EmploiTempsPdfModel model);
    }

    public class PdfGeneratorService : IPdfGeneratorService
    {
        public byte[] GenerateEmploiTempsPdf(EmploiTempsPdfModel model)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Mise en page A4 paysage
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    // En-tête
                    page.Header().Element(header => ComposeHeader(header, model));

                    // Corps (Tableau)
                    page.Content().PaddingVertical(1, Unit.Centimetre).Element(content => ComposeContent(content, model));

                    // Pied de page
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" sur ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private void ComposeHeader(IContainer container, EmploiTempsPdfModel model)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Logo si disponible
                    if (!string.IsNullOrEmpty(model.LogoPath) && File.Exists(model.LogoPath))
                    {
                        row.ConstantItem(80).Image(model.LogoPath);
                    }
                    else
                    {
                        row.ConstantItem(80);
                    }

                    // Textes de l'en-tête (centrés)
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().AlignCenter().Text(model.Etablissement).Bold().FontSize(18).FontColor(Colors.Black);
                        col.Item().AlignCenter().PaddingBottom(15).Text($"EMPLOI DU TEMPS - {model.NiveauNom.ToUpper()}").Bold().FontSize(14);
                        col.Item().AlignLeft().Text($"Semaine : {model.SemaineText}").FontSize(12);
                        col.Item().AlignLeft().Text($"Date de génération : {model.DateGeneration:dd/MM/yyyy}").FontSize(12);
                    });

                    // Espace droit pour équilibrer
                    row.ConstantItem(80);
                });
            });
        }

        private void ComposeContent(IContainer container, EmploiTempsPdfModel model)
        {
            container.Table(table =>
            {
                // Définition des colonnes (1 pour l'heure, 5 pour les jours de la semaine)
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80); // Heure
                    columns.RelativeColumn();   // Lundi
                    columns.RelativeColumn();   // Mardi
                    columns.RelativeColumn();   // Mercredi
                    columns.RelativeColumn();   // Jeudi
                    columns.RelativeColumn();   // Vendredi
                });

                // En-têtes du tableau
                string[] entetes = { "Heure", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };
                foreach (var titre in entetes)
                {
                    table.Cell().Background(Colors.Grey.Lighten3)
                         .Border(1).BorderColor(Colors.Black)
                         .Padding(5).AlignCenter().Text(titre).SemiBold();
                }

                // Définition des créneaux horaires classiques
                var creneaux = new[] 
                {
                    new { Debut = 7, Fin = 8 },
                    new { Debut = 8, Fin = 10 },
                    new { Debut = 10, Fin = 12 },
                    new { Debut = 14, Fin = 16 },
                    new { Debut = 16, Fin = 18 }
                };

                var joursSemaine = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

                // Boucle sur les créneaux horaires
                foreach (var creneau in creneaux)
                {
                    // Colonne "Heure"
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(5)
                         .AlignMiddle().AlignCenter().Text($"{creneau.Debut:00}-{creneau.Fin:00}").Bold();

                    // Colonnes "Jours"
                    foreach (var jour in joursSemaine)
                    {
                        var coursTrouve = model.CoursList.FirstOrDefault(c => 
                            c.HeureDebut.DayOfWeek == jour && 
                            c.HeureDebut.Hour <= creneau.Debut && c.HeureFin.Hour > creneau.Debut);

                        var cell = table.Cell().Border(1).BorderColor(Colors.Black).Padding(4);

                        if (coursTrouve != null)
                        {
                            cell.Column(col =>
                            {
                                col.Item().AlignCenter().Text(coursTrouve.Matiere).Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
                                col.Item().AlignCenter().Text(coursTrouve.Professeur?.Nom ?? "Professeur").FontSize(10).Italic();
                                col.Item().AlignCenter().Text(coursTrouve.Salle?.Nom ?? "Salle").FontSize(10);
                            });
                        }
                        else
                        {
                            cell.AlignCenter().AlignMiddle().Text("-").FontColor(Colors.Grey.Lighten1);
                        }
                    }
                }
            });
        }
    }
}
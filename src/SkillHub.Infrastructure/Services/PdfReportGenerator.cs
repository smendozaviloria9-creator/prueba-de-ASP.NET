using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;

namespace SkillHub.Infrastructure.Services;

public class PdfReportGenerator : IPdfReportGenerator
{
    static PdfReportGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateEnrollmentCertificate(EnrollmentCertificateData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor("#1f2937"));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(titleCol =>
                        {
                            titleCol.Item().Text(data.OrganizationName)
                                .FontSize(26)
                                .Bold()
                                .FontColor("#4f46e5");

                            titleCol.Item().Text("Centro de Formación y Talleres Prácticos")
                                .FontSize(11)
                                .Italic()
                                .FontColor("#6b7280");
                        });

                        row.ConstantItem(120).AlignRight().Column(metaCol =>
                        {
                            metaCol.Item().Background("#eef2ff").Padding(8).Column(badge =>
                            {
                                badge.Item().Text("ESTADO").FontSize(8).Bold().FontColor("#4f46e5");
                                badge.Item().Text("ACTIVA").FontSize(12).Bold().FontColor("#16a34a");
                            });
                        });
                    });

                    col.Item().PaddingTop(15).LineHorizontal(2).LineColor("#4f46e5");
                });

                // Content
                page.Content().PaddingVertical(25).Column(col =>
                {
                    col.Item().AlignCenter().PaddingBottom(20).Text(data.CertificateTitle.ToUpperInvariant())
                        .FontSize(20)
                        .Bold()
                        .FontColor("#111827")
                        .LetterSpacing(1.5f);

                    col.Item().PaddingBottom(15).Text(text =>
                    {
                        text.Span("Por medio del presente documento, ");
                        text.Span(data.OrganizationName).Bold();
                        text.Span(" certifica que la persona indicada a continuación se encuentra debidamente registrada e inscrita en el taller especificado:");
                    });

                    // Participant Section
                    col.Item().PaddingBottom(15).Border(1).BorderColor("#e5e7eb").Background("#f9fafb").Padding(12).Column(partCol =>
                    {
                        partCol.Item().PaddingBottom(6).Text("DATOS DEL PARTICIPANTE")
                            .FontSize(10)
                            .Bold()
                            .FontColor("#4f46e5");

                        partCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("Nombre completo: ").Bold();
                                t.Span(data.ParticipantName);
                            });

                            r.RelativeItem().Text(t =>
                            {
                                t.Span("Correo electrónico: ").Bold();
                                t.Span(data.ParticipantEmail);
                            });
                        });
                    });

                    // Workshop Section
                    col.Item().PaddingBottom(20).Border(1).BorderColor("#e5e7eb").Padding(15).Column(wCol =>
                    {
                        wCol.Item().PaddingBottom(10).Text("DETALLES DEL TALLER")
                            .FontSize(10)
                            .Bold()
                            .FontColor("#4f46e5");

                        wCol.Item().PaddingBottom(6).Row(r =>
                        {
                            r.ConstantItem(140).Text("Código del Taller:").Bold();
                            r.RelativeItem().Text(data.WorkshopCode).Bold().FontColor("#4f46e5");
                        });

                        wCol.Item().PaddingBottom(6).Row(r =>
                        {
                            r.ConstantItem(140).Text("Título:").Bold();
                            r.RelativeItem().Text(data.WorkshopTitle).Bold();
                        });

                        wCol.Item().PaddingBottom(6).Row(r =>
                        {
                            r.ConstantItem(140).Text("Instructor:").Bold();
                            r.RelativeItem().Text(data.InstructorName);
                        });

                        wCol.Item().PaddingBottom(6).Row(r =>
                        {
                            r.ConstantItem(140).Text("Fecha de inicio:").Bold();
                            r.RelativeItem().Text($"{data.WorkshopStartDateTime:dd/MM/yyyy HH:mm} hrs");
                        });

                        wCol.Item().Row(r =>
                        {
                            r.ConstantItem(140).Text("Fecha de finalización:").Bold();
                            r.RelativeItem().Text($"{data.WorkshopEndDateTime:dd/MM/yyyy HH:mm} hrs");
                        });
                    });

                    // Timeline & Verification Section
                    col.Item().Border(1).BorderColor("#e5e7eb").Background("#fdfdfd").Padding(12).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Fecha de inscripción:").FontSize(9).FontColor("#6b7280");
                            c.Item().Text($"{data.EnrolledAt:dd/MM/yyyy HH:mm:ss}").Bold().FontSize(10);
                        });

                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Fecha de emisión:").FontSize(9).FontColor("#6b7280");
                            c.Item().Text($"{data.GeneratedAt:dd/MM/yyyy HH:mm:ss}").Bold().FontSize(10);
                        });

                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Validez:").FontSize(9).FontColor("#6b7280");
                            c.Item().Text("Inscripción Oficial Confirmada").Bold().FontSize(10).FontColor("#16a34a");
                        });
                    });
                });

                // Footer
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor("#e5e7eb");
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Documento emitido automáticamente por la plataforma SkillHub.")
                            .FontSize(8)
                            .Italic()
                            .FontColor("#9ca3af");

                        row.RelativeItem().AlignRight().Text(t =>
                        {
                            t.Span("Página ");
                            t.CurrentPageNumber();
                            t.Span(" de ");
                            t.TotalPages();
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}

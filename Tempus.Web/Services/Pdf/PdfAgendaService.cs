using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Tempus.Core.Models;
using Tempus.Core.Enums;

namespace Tempus.Web.Services.Pdf;

public class PdfAgendaService : IPdfAgendaService
{
    public byte[] GenerateDailyAgenda(List<Event> events, DateTime date, string userName)
    {
        // Configure QuestPDF license (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Inch);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, events, date, userName));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Main title
            column.Item().PaddingBottom(10).Text("Your day at a glance")
                .FontSize(24)
                .Bold()
                .FontColor(Colors.Grey.Darken3);

            // Decorative line
            column.Item().PaddingBottom(20).BorderBottom(2).BorderColor(Colors.Blue.Medium);
        });
    }

    private void ComposeContent(IContainer container, List<Event> events, DateTime date, string userName)
    {
        container.Column(column =>
        {
            // Date and user info section
            column.Item().PaddingBottom(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Date: {date:dddd, MMMM dd, yyyy}")
                        .FontSize(12)
                        .Bold();
                    col.Item().PaddingTop(5).Text($"Prepared for: {userName}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Total Events: {events.Count}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:g}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            // Events section
            if (events.Any())
            {
                // Group events by time
                var sortedEvents = events.OrderBy(e => e.StartTime).ToList();

                foreach (var evt in sortedEvents)
                {
                    column.Item().PaddingBottom(15).Element(c => ComposeEventItem(c, evt));
                }
            }
            else
            {
                // No events message
                column.Item().PaddingVertical(40).AlignCenter().Column(col =>
                {
                    col.Item().Text("No events scheduled for this day")
                        .FontSize(14)
                        .FontColor(Colors.Grey.Medium)
                        .Italic();
                    col.Item().PaddingTop(10).Text("Enjoy your free time!")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Medium);
                });
            }

            // Notes section
            column.Item().PaddingTop(30).Column(col =>
            {
                col.Item().PaddingBottom(10).Text("Notes:")
                    .FontSize(12)
                    .Bold()
                    .FontColor(Colors.Grey.Darken2);

                // Lines for handwritten notes
                for (int i = 0; i < 5; i++)
                {
                    col.Item().PaddingTop(15).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                }
            });
        });
    }

    private void ComposeEventItem(IContainer container, Event evt)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4)
            .Padding(12).Column(column =>
            {
                // Time and title row
                column.Item().Row(row =>
                {
                    // Time column
                    row.ConstantItem(100).Column(timeCol =>
                    {
                        if (evt.IsAllDay)
                        {
                            timeCol.Item().Text("All Day")
                                .FontSize(11)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);
                        }
                        else
                        {
                            timeCol.Item().Text(evt.StartTime.ToString("h:mm tt"))
                                .FontSize(11)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);
                            timeCol.Item().PaddingTop(2).Text(evt.EndTime.ToString("h:mm tt"))
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                    });

                    // Event details column
                    row.RelativeItem().PaddingLeft(15).Column(detailCol =>
                    {
                        // Title and type
                        detailCol.Item().Row(titleRow =>
                        {
                            titleRow.RelativeItem().Text(evt.Title)
                                .FontSize(13)
                                .Bold()
                                .FontColor(Colors.Grey.Darken3);

                            titleRow.ConstantItem(80).AlignRight().Container()
                                .Background(GetEventTypeColor(evt.EventType))
                                .Padding(3)
                                .Text(text =>
                                {
                                    text.Span(evt.EventType.ToString())
                                        .FontSize(8)
                                        .FontColor(Colors.White);
                                });
                        });

                        // Description
                        if (!string.IsNullOrWhiteSpace(evt.Description))
                        {
                            detailCol.Item().PaddingTop(5).Text(evt.Description)
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1)
                                .LineHeight(1.3f);
                        }

                        // Location
                        if (!string.IsNullOrWhiteSpace(evt.Location))
                        {
                            detailCol.Item().PaddingTop(5).Row(locRow =>
                            {
                                locRow.ConstantItem(14).Text("📍")
                                    .FontSize(10);
                                locRow.RelativeItem().Text(evt.Location)
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        }

                        // Attendees
                        if (evt.EventType == EventType.Meeting && evt.Attendees.Any())
                        {
                            detailCol.Item().PaddingTop(5).Row(attendeeRow =>
                            {
                                attendeeRow.ConstantItem(14).Text("👥")
                                    .FontSize(10);
                                attendeeRow.RelativeItem().Text($"{string.Join(", ", evt.Attendees.Select(a => a.Name))}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        }

                        // Priority indicator
                        if (evt.Priority == Priority.High || evt.Priority == Priority.Urgent)
                        {
                            detailCol.Item().PaddingTop(5).Container()
                                .Background(Colors.Red.Lighten3)
                                .Padding(3)
                                .Width(120)
                                .Text(text =>
                                {
                                    text.Span($"⚠ {evt.Priority} Priority")
                                        .FontSize(8)
                                        .Bold()
                                        .FontColor(Colors.Red.Darken2);
                                });
                        }
                    });
                });
            });
    }

    private string GetEventTypeColor(EventType type)
    {
        return type switch
        {
            EventType.Meeting => "#667eea",
            EventType.Appointment => "#4facfe",
            EventType.Task => "#f093fb",
            EventType.TimeBlock => "#43e97b",
            EventType.Reminder => "#fa709a",
            EventType.Deadline => "#ff0844",
            _ => "#718096"
        };
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Generated by ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span("Tempus").FontSize(8).Bold().FontColor(Colors.Blue.Medium);
            text.Span(" - Your Time Management Solution").FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }
}

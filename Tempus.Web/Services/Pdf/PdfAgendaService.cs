using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Tempus.Core.Models;
using Tempus.Core.Enums;

namespace Tempus.Web.Services.Pdf;

public class PdfAgendaService : IPdfAgendaService
{
    private PrintOptions _options = new();

    public byte[] GenerateDailyAgenda(List<Event> events, DateTime date, string userName, PrintOptions? options = null)
    {
        _options = options ?? new PrintOptions();
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

    public byte[] GenerateWeeklyAgenda(List<Event> events, DateTime weekStart, string userName, PrintOptions? options = null)
    {
        _options = options ?? new PrintOptions();
        QuestPDF.Settings.License = LicenseType.Community;

        var weekEnd = weekStart.AddDays(6);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(0.5f, Unit.Inch);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(c => ComposeWeekHeader(c, weekStart, weekEnd, userName));
                page.Content().Element(c => ComposeWeekContent(c, events, weekStart));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeWeekHeader(IContainer container, DateTime weekStart, DateTime weekEnd, string userName)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Weekly Agenda")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(5).Text($"{weekStart:MMMM dd} - {weekEnd:MMMM dd, yyyy}")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Prepared for: {userName}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text($"Generated: {DateTime.Now:g}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            column.Item().PaddingTop(10).PaddingBottom(10).BorderBottom(2).BorderColor(Colors.Blue.Medium);
        });
    }

    private void ComposeWeekContent(IContainer container, List<Event> events, DateTime weekStart)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (int i = 0; i < 7; i++)
                    columns.RelativeColumn();
            });

            // Header row with day names
            for (int i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var isToday = day.Date == DateTime.Today;

                table.Cell().Row(1).Column((uint)(i + 1))
                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                    .Background(isToday ? Colors.Blue.Lighten4 : Colors.Grey.Lighten4)
                    .Padding(5)
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text(day.ToString("ddd"))
                            .FontSize(10)
                            .Bold()
                            .FontColor(isToday ? Colors.Blue.Darken2 : Colors.Grey.Darken2);
                        col.Item().AlignCenter().Text(day.ToString("MMM d"))
                            .FontSize(9)
                            .FontColor(isToday ? Colors.Blue.Darken1 : Colors.Grey.Darken1);
                    });
            }

            // Content row with events
            for (int i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var dayEvents = events.Where(e => e.StartTime.Date == day.Date)
                    .OrderBy(e => e.StartTime)
                    .ToList();

                table.Cell().Row(2).Column((uint)(i + 1))
                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                    .MinHeight(200)
                    .Padding(3)
                    .Column(col =>
                    {
                        if (dayEvents.Any())
                        {
                            foreach (var evt in dayEvents)
                            {
                                col.Item().PaddingBottom(4).Element(c => ComposeCompactEvent(c, evt));
                            }
                        }
                        else
                        {
                            col.Item().AlignCenter().PaddingTop(20).Text("No events")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Medium)
                                .Italic();
                        }
                    });
            }
        });
    }

    private void ComposeCompactEvent(IContainer container, Event evt)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2)
            .Background(GetEventTypeColor(evt.EventType))
            .Padding(3)
            .Column(col =>
            {
                // Time
                if (!evt.IsAllDay)
                {
                    col.Item().Text(evt.StartTime.ToString(_options.TimeFormat == "24" ? "HH:mm" : "h:mm tt"))
                        .FontSize(7)
                        .FontColor(Colors.White);
                }
                else
                {
                    col.Item().Text("All Day")
                        .FontSize(7)
                        .FontColor(Colors.White);
                }

                // Title
                col.Item().Text(evt.Title)
                    .FontSize(8)
                    .Bold()
                    .FontColor(Colors.White)
                    .ClampLines(2);

                // Location (if enabled and exists)
                if (_options.IncludeLocation && !string.IsNullOrWhiteSpace(evt.Location))
                {
                    col.Item().Text(evt.Location)
                        .FontSize(7)
                        .FontColor(Colors.White)
                        .ClampLines(1);
                }
            });
    }

    public byte[] GenerateMonthlyCalendar(List<Event> events, int month, int year, string userName, PrintOptions? options = null)
    {
        _options = options ?? new PrintOptions();
        QuestPDF.Settings.License = LicenseType.Community;

        var firstDay = new DateTime(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(0.5f, Unit.Inch);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Element(c => ComposeMonthHeader(c, firstDay, userName));
                page.Content().Element(c => ComposeMonthContent(c, events, firstDay, lastDay));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeMonthHeader(IContainer container, DateTime month, string userName)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(month.ToString("MMMM yyyy"))
                    .FontSize(24)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Prepared for: {userName}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text($"Generated: {DateTime.Now:g}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            column.Item().PaddingTop(10).PaddingBottom(10).BorderBottom(2).BorderColor(Colors.Blue.Medium);
        });
    }

    private void ComposeMonthContent(IContainer container, List<Event> events, DateTime firstDay, DateTime lastDay)
    {
        // Calculate first day of calendar grid (previous Sunday or Monday)
        var startOfWeek = DayOfWeek.Sunday;
        var calendarStart = firstDay;
        while (calendarStart.DayOfWeek != startOfWeek)
            calendarStart = calendarStart.AddDays(-1);

        // Calculate number of weeks needed
        var calendarEnd = lastDay;
        while (calendarEnd.DayOfWeek != DayOfWeek.Saturday)
            calendarEnd = calendarEnd.AddDays(1);

        var totalDays = (calendarEnd - calendarStart).Days + 1;
        var totalWeeks = totalDays / 7;

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (int i = 0; i < 7; i++)
                    columns.RelativeColumn();
            });

            // Day of week headers
            string[] dayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            for (int i = 0; i < 7; i++)
            {
                table.Cell().Row(1).Column((uint)(i + 1))
                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                    .Background(Colors.Grey.Darken2)
                    .Padding(5)
                    .AlignCenter()
                    .Text(dayNames[i])
                    .FontSize(10)
                    .Bold()
                    .FontColor(Colors.White);
            }

            // Calendar days
            var currentDate = calendarStart;
            for (int week = 0; week < totalWeeks; week++)
            {
                for (int day = 0; day < 7; day++)
                {
                    var cellDate = currentDate;
                    var isCurrentMonth = cellDate.Month == firstDay.Month;
                    var isToday = cellDate.Date == DateTime.Today;
                    var dayEvents = events.Where(e => e.StartTime.Date == cellDate.Date)
                        .OrderBy(e => e.StartTime)
                        .ToList();

                    table.Cell().Row((uint)(week + 2)).Column((uint)(day + 1))
                        .Border(1).BorderColor(Colors.Grey.Lighten2)
                        .Background(isToday ? Colors.Blue.Lighten4 : (isCurrentMonth ? Colors.White : Colors.Grey.Lighten4))
                        .MinHeight(80)
                        .Padding(3)
                        .Column(col =>
                        {
                            // Day number
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text(cellDate.Day.ToString())
                                    .FontSize(isToday ? 12 : 10)
                                    .Bold()
                                    .FontColor(isToday ? Colors.Blue.Darken2 : (isCurrentMonth ? Colors.Grey.Darken2 : Colors.Grey.Medium));

                                if (dayEvents.Count > 3)
                                {
                                    r.ConstantItem(30).AlignRight().Text($"+{dayEvents.Count - 3}")
                                        .FontSize(7)
                                        .FontColor(Colors.Grey.Medium);
                                }
                            });

                            // Events (max 3)
                            foreach (var evt in dayEvents.Take(3))
                            {
                                col.Item().PaddingTop(2).Element(c => ComposeMonthEvent(c, evt));
                            }
                        });

                    currentDate = currentDate.AddDays(1);
                }
            }
        });
    }

    private void ComposeMonthEvent(IContainer container, Event evt)
    {
        container.Background(GetEventTypeColor(evt.EventType))
            .Padding(2)
            .Row(row =>
            {
                if (!evt.IsAllDay)
                {
                    row.ConstantItem(35).Text(evt.StartTime.ToString(_options.TimeFormat == "24" ? "HH:mm" : "h:mm"))
                        .FontSize(6)
                        .FontColor(Colors.White);
                }

                row.RelativeItem().Text(evt.Title)
                    .FontSize(7)
                    .FontColor(Colors.White)
                    .ClampLines(1);
            });
    }

    public byte[] GenerateDateRangeAgenda(List<Event> events, DateTime startDate, DateTime endDate, string userName, PrintOptions? options = null)
    {
        _options = options ?? new PrintOptions();
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(0.75f, Unit.Inch);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => ComposeDateRangeHeader(c, startDate, endDate, userName, events.Count));
                page.Content().Element(c => ComposeDateRangeContent(c, events, startDate, endDate));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeDateRangeHeader(IContainer container, DateTime startDate, DateTime endDate, string userName, int eventCount)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Agenda")
                        .FontSize(22)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(5).Text($"{startDate:MMMM dd, yyyy} - {endDate:MMMM dd, yyyy}")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Prepared for: {userName}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text($"Total Events: {eventCount}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(3).Text($"Generated: {DateTime.Now:g}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            column.Item().PaddingTop(10).PaddingBottom(15).BorderBottom(2).BorderColor(Colors.Blue.Medium);
        });
    }

    private void ComposeDateRangeContent(IContainer container, List<Event> events, DateTime startDate, DateTime endDate)
    {
        container.Column(column =>
        {
            // Group events by date
            var groupedEvents = events
                .OrderBy(e => e.StartTime)
                .GroupBy(e => e.StartTime.Date)
                .ToList();

            if (!groupedEvents.Any())
            {
                column.Item().PaddingVertical(40).AlignCenter().Text("No events scheduled for this period")
                    .FontSize(14)
                    .FontColor(Colors.Grey.Medium)
                    .Italic();
                return;
            }

            foreach (var dayGroup in groupedEvents)
            {
                // Day header
                var isToday = dayGroup.Key == DateTime.Today;
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.ConstantItem(120).Background(isToday ? Colors.Blue.Medium : Colors.Grey.Darken1)
                        .Padding(5)
                        .Column(col =>
                        {
                            col.Item().Text(dayGroup.Key.ToString("dddd"))
                                .FontSize(10)
                                .Bold()
                                .FontColor(Colors.White);
                            col.Item().Text(dayGroup.Key.ToString("MMM d, yyyy"))
                                .FontSize(9)
                                .FontColor(Colors.White);
                        });

                    row.RelativeItem().PaddingLeft(10).AlignBottom().Text($"{dayGroup.Count()} event(s)")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });

                // Events for this day
                foreach (var evt in dayGroup)
                {
                    column.Item().PaddingTop(8).PaddingLeft(10).Element(c => ComposeEventItem(c, evt));
                }
            }

            // Notes section if enabled
            if (_options.IncludeNotesSection)
            {
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().PaddingBottom(10).Text("Notes:")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Grey.Darken2);

                    for (int i = 0; i < 4; i++)
                    {
                        col.Item().PaddingTop(15).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    }
                });
            }
        });
    }
}

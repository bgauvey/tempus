using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using System.Text;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class AnalyticsReportService : IAnalyticsReportService
{
    public AnalyticsReportService()
    {
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GeneratePdfReportAsync(CalendarAnalytics analytics, string userName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Text("Tempus Analytics Report").FontSize(20).SemiBold();
                        column.Item().Text($"Generated for: {userName}").FontSize(10);
                        column.Item().Text($"Period: {analytics.StartDate:MMM dd, yyyy} - {analytics.EndDate:MMM dd, yyyy}").FontSize(9);
                        column.Item().Text($"Generated: {DateTime.Now:MMM dd, yyyy HH:mm:ss}").FontSize(8);
                        column.Item().PaddingVertical(5);
                    });

                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Overview Section
                        column.Item().Text("Overview").FontSize(14).SemiBold();
                        column.Item().Text($"Total Events: {analytics.TotalEvents}");
                        column.Item().Text($"Total Meeting Cost: ${analytics.TotalMeetingCost:F2}");
                        column.Item().Text($"Report Period: {(analytics.EndDate - analytics.StartDate).Days} days");
                        column.Item().PaddingVertical(5);

                        // Calendar Health Score
                        column.Item().Text("Calendar Health Score").FontSize(14).SemiBold();
                        column.Item().Text($"Score: {analytics.Productivity.CalendarHealthScore:F1}/100").FontSize(16);
                        column.Item().Text(GetHealthScoreDescription(analytics.Productivity.CalendarHealthScore)).FontSize(9);
                        column.Item().PaddingVertical(5);

                        // Time Usage
                        column.Item().Text("Time Usage Analysis").FontSize(14).SemiBold();
                        column.Item().Text($"Total Scheduled: {analytics.TimeUsage.TotalScheduledHours} hours ({analytics.TimeUsage.ScheduledPercentage:F1}%)");
                        column.Item().Text($"Available Time: {analytics.TimeUsage.TotalFreeHours} hours");

                        if (analytics.TimeUsage.EventTypeHours.Any())
                        {
                            column.Item().Text("Time by Event Type:").FontSize(10).SemiBold();
                            foreach (var type in analytics.TimeUsage.EventTypeHours.OrderByDescending(x => x.Value).Take(5))
                            {
                                var percentage = analytics.TimeUsage.TotalScheduledHours > 0
                                    ? (type.Value / (double)analytics.TimeUsage.TotalScheduledHours * 100)
                                    : 0;
                                column.Item().Text($"  • {type.Key}: {type.Value} hours ({percentage:F1}%)").FontSize(9);
                            }
                        }
                        column.Item().PaddingVertical(5);

                        // Meeting Analytics
                        column.Item().Text("Meeting Analytics").FontSize(14).SemiBold();
                        column.Item().Text($"Total Meetings: {analytics.MeetingStats.TotalMeetings}");
                        column.Item().Text($"Total Meeting Hours: {analytics.MeetingStats.TotalMeetingHours}");
                        column.Item().Text($"Average Duration: {analytics.MeetingStats.AverageMeetingDuration:F0} minutes");
                        column.Item().Text($"Average Cost: ${analytics.MeetingStats.AverageMeetingCost:F2}");
                        column.Item().Text($"Back-to-Back Meetings: {analytics.MeetingStats.BackToBackMeetings}");
                        column.Item().PaddingVertical(5);

                        // Productivity Metrics
                        column.Item().Text("Productivity Metrics").FontSize(14).SemiBold();
                        column.Item().Text($"Focus Time Blocks: {analytics.Productivity.FocusTimeBlocks}");
                        column.Item().Text($"Total Focus Hours: {analytics.Productivity.TotalFocusHours}");
                        column.Item().Text($"Task Completion Rate: {analytics.Productivity.TaskCompletionRate:F1}%");
                        column.Item().Text($"Fragmented Hours: {analytics.Productivity.FragmentedHours}");
                        column.Item().PaddingVertical(5);

                        // Recommendations
                        if (analytics.Productivity.Recommendations.Any())
                        {
                            column.Item().Text("Recommendations").FontSize(14).SemiBold();
                            foreach (var recommendation in analytics.Productivity.Recommendations)
                            {
                                column.Item().Text($"• {recommendation}").FontSize(9);
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateCsvReportAsync(CalendarAnalytics analytics)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Tempus Analytics Report - CSV Export");
        csv.AppendLine($"Period,{analytics.StartDate:yyyy-MM-dd},{analytics.EndDate:yyyy-MM-dd}");
        csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();

        // Overview
        csv.AppendLine("Overview");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Events,{analytics.TotalEvents}");
        csv.AppendLine($"Total Meeting Cost,${analytics.TotalMeetingCost:F2}");
        csv.AppendLine($"Calendar Health Score,{analytics.Productivity.CalendarHealthScore:F1}");
        csv.AppendLine();

        // Time Usage
        csv.AppendLine("Time Usage");
        csv.AppendLine("Metric,Hours,Percentage");
        csv.AppendLine($"Total Scheduled Time,{analytics.TimeUsage.TotalScheduledHours},{analytics.TimeUsage.ScheduledPercentage:F1}%");
        csv.AppendLine($"Available Time,{analytics.TimeUsage.TotalFreeHours},{(100 - analytics.TimeUsage.ScheduledPercentage):F1}%");
        csv.AppendLine();

        // Time by Type
        csv.AppendLine("Time by Event Type");
        csv.AppendLine("Type,Hours,Percentage");
        foreach (var type in analytics.TimeUsage.EventTypeHours.OrderByDescending(x => x.Value))
        {
            var percentage = analytics.TimeUsage.TotalScheduledHours > 0
                ? (type.Value / (double)analytics.TimeUsage.TotalScheduledHours * 100)
                : 0;
            csv.AppendLine($"{type.Key},{type.Value},{percentage:F1}%");
        }
        csv.AppendLine();

        // Peak Hours
        csv.AppendLine("Busiest Hours of Day");
        csv.AppendLine("Hour,Events");
        foreach (var hour in analytics.TimeUsage.HourOfDayDistribution.OrderByDescending(x => x.Value).Take(10))
        {
            csv.AppendLine($"{hour.Key:D2}:00,{hour.Value}");
        }
        csv.AppendLine();

        // Meeting Analytics
        csv.AppendLine("Meeting Analytics");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Meetings,{analytics.MeetingStats.TotalMeetings}");
        csv.AppendLine($"Total Meeting Hours,{analytics.MeetingStats.TotalMeetingHours}");
        csv.AppendLine($"Average Duration (min),{analytics.MeetingStats.AverageMeetingDuration:F0}");
        csv.AppendLine($"Average Meeting Cost,${analytics.MeetingStats.AverageMeetingCost:F2}");
        csv.AppendLine($"Back-to-Back Meetings,{analytics.MeetingStats.BackToBackMeetings}");
        csv.AppendLine();

        // Top Attendees
        if (analytics.MeetingStats.TopAttendees.Any())
        {
            csv.AppendLine("Top Meeting Attendees");
            csv.AppendLine("Name,Email,Meeting Count");
            foreach (var attendee in analytics.MeetingStats.TopAttendees.Take(10))
            {
                csv.AppendLine($"\"{attendee.Name}\",\"{attendee.Email}\",{attendee.MeetingCount}");
            }
            csv.AppendLine();
        }

        // Productivity Metrics
        csv.AppendLine("Productivity Metrics");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Focus Time Blocks,{analytics.Productivity.FocusTimeBlocks}");
        csv.AppendLine($"Total Focus Hours,{analytics.Productivity.TotalFocusHours}");
        csv.AppendLine($"Task Completion Rate,{analytics.Productivity.TaskCompletionRate:F1}%");
        csv.AppendLine($"Fragmented Hours,{analytics.Productivity.FragmentedHours}");
        csv.AppendLine();

        // Recommendations
        if (analytics.Productivity.Recommendations.Any())
        {
            csv.AppendLine("Recommendations");
            for (int i = 0; i < analytics.Productivity.Recommendations.Count; i++)
            {
                csv.AppendLine($"{i + 1},\"{analytics.Productivity.Recommendations[i]}\"");
            }
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return Task.FromResult(bytes);
    }

    public Task<byte[]> GenerateExcelReportAsync(CalendarAnalytics analytics, string userName)
    {
        using var workbook = new XLWorkbook();

        // Overview Sheet
        var overviewSheet = workbook.Worksheets.Add("Overview");
        CreateOverviewSheet(overviewSheet, analytics, userName);

        // Time Usage Sheet
        var timeSheet = workbook.Worksheets.Add("Time Usage");
        CreateTimeUsageSheet(timeSheet, analytics);

        // Meeting Analytics Sheet
        var meetingSheet = workbook.Worksheets.Add("Meeting Analytics");
        CreateMeetingSheet(meetingSheet, analytics);

        // Productivity Sheet
        var productivitySheet = workbook.Worksheets.Add("Productivity");
        CreateProductivitySheet(productivitySheet, analytics);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    private string GetHealthScoreDescription(double score)
    {
        return score switch
        {
            >= 80 => "Excellent - Your calendar is well-balanced and optimized for productivity.",
            >= 70 => "Good - Your calendar is generally well-organized with room for minor improvements.",
            >= 50 => "Fair - Consider optimizing your schedule to improve work-life balance.",
            >= 30 => "Poor - Your schedule shows signs of overcommitment and fragmentation.",
            _ => "Critical - Immediate action needed to prevent burnout and improve effectiveness."
        };
    }

    // Excel Helper Methods
    private void CreateOverviewSheet(IXLWorksheet sheet, CalendarAnalytics analytics, string userName)
    {
        sheet.Cell("A1").Value = "Tempus Analytics Report";
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 16;

        sheet.Cell("A2").Value = $"Generated for: {userName}";
        sheet.Cell("A3").Value = $"Period: {analytics.StartDate:MMM dd, yyyy} - {analytics.EndDate:MMM dd, yyyy}";
        sheet.Cell("A4").Value = $"Generated: {DateTime.Now:MMM dd, yyyy HH:mm:ss}";

        sheet.Cell("A6").Value = "Overview Metrics";
        sheet.Cell("A6").Style.Font.Bold = true;

        var row = 7;
        sheet.Cell($"A{row}").Value = "Total Events";
        sheet.Cell($"B{row}").Value = analytics.TotalEvents;

        row++;
        sheet.Cell($"A{row}").Value = "Total Meeting Cost";
        sheet.Cell($"B{row}").Value = (double)analytics.TotalMeetingCost;
        sheet.Cell($"B{row}").Style.NumberFormat.Format = "$#,##0.00";

        row++;
        sheet.Cell($"A{row}").Value = "Calendar Health Score";
        sheet.Cell($"B{row}").Value = analytics.Productivity.CalendarHealthScore;
        sheet.Cell($"B{row}").Style.NumberFormat.Format = "0.0";

        row++;
        sheet.Cell($"A{row}").Value = "Health Status";
        sheet.Cell($"B{row}").Value = GetHealthScoreDescription(analytics.Productivity.CalendarHealthScore);

        sheet.Columns().AdjustToContents();
    }

    private void CreateTimeUsageSheet(IXLWorksheet sheet, CalendarAnalytics analytics)
    {
        sheet.Cell("A1").Value = "Time Usage Analysis";
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 14;

        var row = 3;
        sheet.Cell($"A{row}").Value = "Metric";
        sheet.Cell($"B{row}").Value = "Hours";
        sheet.Cell($"C{row}").Value = "Percentage";
        sheet.Range($"A{row}:C{row}").Style.Font.Bold = true;
        sheet.Range($"A{row}:C{row}").Style.Fill.BackgroundColor = XLColor.LightGray;

        row++;
        sheet.Cell($"A{row}").Value = "Total Scheduled Time";
        sheet.Cell($"B{row}").Value = analytics.TimeUsage.TotalScheduledHours;
        sheet.Cell($"C{row}").Value = analytics.TimeUsage.ScheduledPercentage / 100;
        sheet.Cell($"C{row}").Style.NumberFormat.Format = "0.0%";

        row++;
        sheet.Cell($"A{row}").Value = "Available Time";
        sheet.Cell($"B{row}").Value = analytics.TimeUsage.TotalFreeHours;
        sheet.Cell($"C{row}").Value = (100 - analytics.TimeUsage.ScheduledPercentage) / 100;
        sheet.Cell($"C{row}").Style.NumberFormat.Format = "0.0%";

        // Time by Event Type
        if (analytics.TimeUsage.EventTypeHours.Any())
        {
            row += 2;
            sheet.Cell($"A{row}").Value = "Time by Event Type";
            sheet.Cell($"A{row}").Style.Font.Bold = true;

            row++;
            sheet.Cell($"A{row}").Value = "Type";
            sheet.Cell($"B{row}").Value = "Hours";
            sheet.Cell($"C{row}").Value = "Percentage";
            sheet.Range($"A{row}:C{row}").Style.Font.Bold = true;
            sheet.Range($"A{row}:C{row}").Style.Fill.BackgroundColor = XLColor.LightGray;

            foreach (var type in analytics.TimeUsage.EventTypeHours.OrderByDescending(x => x.Value))
            {
                row++;
                var percentage = analytics.TimeUsage.TotalScheduledHours > 0
                    ? (type.Value / (double)analytics.TimeUsage.TotalScheduledHours)
                    : 0;

                sheet.Cell($"A{row}").Value = type.Key;
                sheet.Cell($"B{row}").Value = type.Value;
                sheet.Cell($"C{row}").Value = percentage;
                sheet.Cell($"C{row}").Style.NumberFormat.Format = "0.0%";
            }
        }

        sheet.Columns().AdjustToContents();
    }

    private void CreateMeetingSheet(IXLWorksheet sheet, CalendarAnalytics analytics)
    {
        sheet.Cell("A1").Value = "Meeting Analytics";
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 14;

        var row = 3;
        sheet.Cell($"A{row}").Value = "Total Meetings";
        sheet.Cell($"B{row}").Value = analytics.MeetingStats.TotalMeetings;

        row++;
        sheet.Cell($"A{row}").Value = "Total Meeting Hours";
        sheet.Cell($"B{row}").Value = analytics.MeetingStats.TotalMeetingHours;

        row++;
        sheet.Cell($"A{row}").Value = "Average Duration (minutes)";
        sheet.Cell($"B{row}").Value = analytics.MeetingStats.AverageMeetingDuration;
        sheet.Cell($"B{row}").Style.NumberFormat.Format = "0.0";

        row++;
        sheet.Cell($"A{row}").Value = "Average Meeting Cost";
        sheet.Cell($"B{row}").Value = (double)analytics.MeetingStats.AverageMeetingCost;
        sheet.Cell($"B{row}").Style.NumberFormat.Format = "$#,##0.00";

        row++;
        sheet.Cell($"A{row}").Value = "Back-to-Back Meetings";
        sheet.Cell($"B{row}").Value = analytics.MeetingStats.BackToBackMeetings;

        // Top Attendees
        if (analytics.MeetingStats.TopAttendees.Any())
        {
            row += 2;
            sheet.Cell($"A{row}").Value = "Top Meeting Attendees";
            sheet.Cell($"A{row}").Style.Font.Bold = true;

            row++;
            sheet.Cell($"A{row}").Value = "Name";
            sheet.Cell($"B{row}").Value = "Email";
            sheet.Cell($"C{row}").Value = "Meeting Count";
            sheet.Range($"A{row}:C{row}").Style.Font.Bold = true;
            sheet.Range($"A{row}:C{row}").Style.Fill.BackgroundColor = XLColor.LightGray;

            foreach (var attendee in analytics.MeetingStats.TopAttendees.Take(20))
            {
                row++;
                sheet.Cell($"A{row}").Value = attendee.Name;
                sheet.Cell($"B{row}").Value = attendee.Email;
                sheet.Cell($"C{row}").Value = attendee.MeetingCount;
            }
        }

        sheet.Columns().AdjustToContents();
    }

    private void CreateProductivitySheet(IXLWorksheet sheet, CalendarAnalytics analytics)
    {
        sheet.Cell("A1").Value = "Productivity Metrics";
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 14;

        var row = 3;
        sheet.Cell($"A{row}").Value = "Focus Time Blocks";
        sheet.Cell($"B{row}").Value = analytics.Productivity.FocusTimeBlocks;

        row++;
        sheet.Cell($"A{row}").Value = "Total Focus Hours";
        sheet.Cell($"B{row}").Value = analytics.Productivity.TotalFocusHours;

        row++;
        sheet.Cell($"A{row}").Value = "Task Completion Rate";
        sheet.Cell($"B{row}").Value = analytics.Productivity.TaskCompletionRate / 100;
        sheet.Cell($"B{row}").Style.NumberFormat.Format = "0.0%";

        row++;
        sheet.Cell($"A{row}").Value = "Fragmented Hours";
        sheet.Cell($"B{row}").Value = analytics.Productivity.FragmentedHours;

        // Recommendations
        if (analytics.Productivity.Recommendations.Any())
        {
            row += 2;
            sheet.Cell($"A{row}").Value = "Recommendations";
            sheet.Cell($"A{row}").Style.Font.Bold = true;

            foreach (var recommendation in analytics.Productivity.Recommendations)
            {
                row++;
                sheet.Cell($"A{row}").Value = $"• {recommendation}";
            }
        }

        sheet.Columns().AdjustToContents();
    }
}

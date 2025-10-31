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

    public Task<byte[]> GeneratePdfReportAsync(CalendarAnalytics analytics, string userName, TrendAnalysis? trendAnalysis = null)
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

                        // Trend Analysis and Predictions
                        if (trendAnalysis != null)
                        {
                            column.Item().PaddingTop(10).Text("Predictive Analytics & Trends").FontSize(14).SemiBold();
                            column.Item().Text($"Workload Pattern: {trendAnalysis.WorkloadPattern}").FontSize(10);
                            column.Item().PaddingVertical(5);

                            // Historical Trends
                            if (trendAnalysis.Trends.Any())
                            {
                                column.Item().Text("Historical Trends:").FontSize(12).SemiBold();
                                foreach (var trend in trendAnalysis.Trends.Take(5))
                                {
                                    var direction = trend.Direction.ToString();
                                    column.Item().Text($"• {trend.MetricName}: {direction} ({trend.ChangePercentage:F1}%)").FontSize(9);
                                }
                                column.Item().PaddingVertical(3);
                            }

                            // Future Predictions
                            if (trendAnalysis.Predictions.Any())
                            {
                                column.Item().Text("Forecasts (Next 30 days):").FontSize(12).SemiBold();
                                foreach (var prediction in trendAnalysis.Predictions.Take(3))
                                {
                                    column.Item().Text($"• {prediction.MetricName}: {prediction.PredictedValue:F1} (Confidence: {prediction.ConfidenceLevel:F0}%)").FontSize(9);
                                }
                                column.Item().PaddingVertical(3);
                            }

                            // Detected Patterns
                            if (trendAnalysis.DetectedPatterns.Any())
                            {
                                column.Item().Text("Detected Patterns:").FontSize(12).SemiBold();
                                foreach (var pattern in trendAnalysis.DetectedPatterns.Take(3))
                                {
                                    column.Item().Text($"• {pattern}").FontSize(9);
                                }
                            }

                            // Key Insights
                            if (trendAnalysis.KeyInsights.Any())
                            {
                                column.Item().PaddingVertical(3);
                                column.Item().Text("Key Insights:").FontSize(12).SemiBold();
                                foreach (var insight in trendAnalysis.KeyInsights)
                                {
                                    column.Item().Text($"• {insight}").FontSize(9);
                                }
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

    public Task<byte[]> GenerateCsvReportAsync(CalendarAnalytics analytics, TrendAnalysis? trendAnalysis = null)
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
            csv.AppendLine();
        }

        // Predictive Analytics & Trends
        if (trendAnalysis != null)
        {
            csv.AppendLine("Predictive Analytics & Trends");
            csv.AppendLine($"Workload Pattern,{trendAnalysis.WorkloadPattern}");
            csv.AppendLine($"Analysis Date,{trendAnalysis.AnalysisDate:yyyy-MM-dd}");
            csv.AppendLine($"Historical Days,{trendAnalysis.HistoricalDays}");
            csv.AppendLine($"Forecast Days,{trendAnalysis.ForecastDays}");
            csv.AppendLine();

            // Historical Trends
            if (trendAnalysis.Trends.Any())
            {
                csv.AppendLine("Historical Trends");
                csv.AppendLine("Metric,Direction,Change %,Average,Min,Max");
                foreach (var trend in trendAnalysis.Trends)
                {
                    csv.AppendLine($"\"{trend.MetricName}\",{trend.Direction},{trend.ChangePercentage:F1},{trend.Average:F1},{trend.Min:F1},{trend.Max:F1}");
                }
                csv.AppendLine();
            }

            // Future Predictions
            if (trendAnalysis.Predictions.Any())
            {
                csv.AppendLine("Future Predictions");
                csv.AppendLine("Metric,Predicted Value,Confidence %");
                foreach (var prediction in trendAnalysis.Predictions)
                {
                    csv.AppendLine($"\"{prediction.MetricName}\",{prediction.PredictedValue:F1},{prediction.ConfidenceLevel:F0}");
                }
                csv.AppendLine();
            }

            // Detected Patterns
            if (trendAnalysis.DetectedPatterns.Any())
            {
                csv.AppendLine("Detected Patterns");
                foreach (var pattern in trendAnalysis.DetectedPatterns)
                {
                    csv.AppendLine($"\"{pattern}\"");
                }
                csv.AppendLine();
            }

            // Key Insights
            if (trendAnalysis.KeyInsights.Any())
            {
                csv.AppendLine("Key Insights");
                foreach (var insight in trendAnalysis.KeyInsights)
                {
                    csv.AppendLine($"\"{insight}\"");
                }
                csv.AppendLine();
            }
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return Task.FromResult(bytes);
    }

    public Task<byte[]> GenerateExcelReportAsync(CalendarAnalytics analytics, string userName, TrendAnalysis? trendAnalysis = null)
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

        // Trend Analysis Sheet
        if (trendAnalysis != null)
        {
            var trendSheet = workbook.Worksheets.Add("Trend Analysis");
            CreateTrendAnalysisSheet(trendSheet, trendAnalysis);
        }

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

    private void CreateTrendAnalysisSheet(IXLWorksheet sheet, TrendAnalysis trendAnalysis)
    {
        sheet.Cell("A1").Value = "Predictive Analytics & Trend Analysis";
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 14;

        var row = 3;
        sheet.Cell($"A{row}").Value = "Workload Pattern";
        sheet.Cell($"B{row}").Value = trendAnalysis.WorkloadPattern;

        row++;
        sheet.Cell($"A{row}").Value = "Analysis Date";
        sheet.Cell($"B{row}").Value = trendAnalysis.AnalysisDate;
        sheet.Cell($"B{row}").Style.DateFormat.Format = "yyyy-MM-dd HH:mm";

        row++;
        sheet.Cell($"A{row}").Value = "Historical Days";
        sheet.Cell($"B{row}").Value = trendAnalysis.HistoricalDays;

        row++;
        sheet.Cell($"A{row}").Value = "Forecast Days";
        sheet.Cell($"B{row}").Value = trendAnalysis.ForecastDays;

        // Historical Trends Section
        if (trendAnalysis.Trends.Any())
        {
            row += 2;
            sheet.Cell($"A{row}").Value = "Historical Trends";
            sheet.Cell($"A{row}").Style.Font.Bold = true;
            sheet.Cell($"A{row}").Style.Font.FontSize = 12;

            row++;
            sheet.Cell($"A{row}").Value = "Metric";
            sheet.Cell($"B{row}").Value = "Direction";
            sheet.Cell($"C{row}").Value = "Change %";
            sheet.Cell($"D{row}").Value = "Average";
            sheet.Cell($"E{row}").Value = "Min";
            sheet.Cell($"F{row}").Value = "Max";
            sheet.Range($"A{row}:F{row}").Style.Font.Bold = true;
            sheet.Range($"A{row}:F{row}").Style.Fill.BackgroundColor = XLColor.LightBlue;

            foreach (var trend in trendAnalysis.Trends)
            {
                row++;
                sheet.Cell($"A{row}").Value = trend.MetricName;
                sheet.Cell($"B{row}").Value = trend.Direction.ToString();
                sheet.Cell($"C{row}").Value = trend.ChangePercentage;
                sheet.Cell($"C{row}").Style.NumberFormat.Format = "0.0";
                sheet.Cell($"D{row}").Value = trend.Average;
                sheet.Cell($"D{row}").Style.NumberFormat.Format = "0.0";
                sheet.Cell($"E{row}").Value = trend.Min;
                sheet.Cell($"E{row}").Style.NumberFormat.Format = "0.0";
                sheet.Cell($"F{row}").Value = trend.Max;
                sheet.Cell($"F{row}").Style.NumberFormat.Format = "0.0";

                // Color code based on direction
                if (trend.Direction == TrendDirection.Increasing)
                {
                    sheet.Cell($"B{row}").Style.Font.FontColor = XLColor.Green;
                }
                else if (trend.Direction == TrendDirection.Decreasing)
                {
                    sheet.Cell($"B{row}").Style.Font.FontColor = XLColor.Red;
                }
            }
        }

        // Future Predictions Section
        if (trendAnalysis.Predictions.Any())
        {
            row += 2;
            sheet.Cell($"A{row}").Value = "Future Predictions";
            sheet.Cell($"A{row}").Style.Font.Bold = true;
            sheet.Cell($"A{row}").Style.Font.FontSize = 12;

            row++;
            sheet.Cell($"A{row}").Value = "Metric";
            sheet.Cell($"B{row}").Value = "Predicted Value";
            sheet.Cell($"C{row}").Value = "Confidence %";
            sheet.Range($"A{row}:C{row}").Style.Font.Bold = true;
            sheet.Range($"A{row}:C{row}").Style.Fill.BackgroundColor = XLColor.LightGreen;

            foreach (var prediction in trendAnalysis.Predictions)
            {
                row++;
                sheet.Cell($"A{row}").Value = prediction.MetricName;
                sheet.Cell($"B{row}").Value = prediction.PredictedValue;
                sheet.Cell($"B{row}").Style.NumberFormat.Format = "0.0";
                sheet.Cell($"C{row}").Value = prediction.ConfidenceLevel;
                sheet.Cell($"C{row}").Style.NumberFormat.Format = "0";
            }
        }

        // Detected Patterns Section
        if (trendAnalysis.DetectedPatterns.Any())
        {
            row += 2;
            sheet.Cell($"A{row}").Value = "Detected Patterns";
            sheet.Cell($"A{row}").Style.Font.Bold = true;
            sheet.Cell($"A{row}").Style.Font.FontSize = 12;

            foreach (var pattern in trendAnalysis.DetectedPatterns)
            {
                row++;
                sheet.Cell($"A{row}").Value = $"• {pattern}";
            }
        }

        // Key Insights Section
        if (trendAnalysis.KeyInsights.Any())
        {
            row += 2;
            sheet.Cell($"A{row}").Value = "Key Insights";
            sheet.Cell($"A{row}").Style.Font.Bold = true;
            sheet.Cell($"A{row}").Style.Font.FontSize = 12;

            foreach (var insight in trendAnalysis.KeyInsights)
            {
                row++;
                sheet.Cell($"A{row}").Value = $"• {insight}";
            }
        }

        sheet.Columns().AdjustToContents();
    }
}

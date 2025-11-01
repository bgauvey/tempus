using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class TrendForecastService : ITrendForecastService
{
    private readonly IEventRepository _eventRepository;

    public TrendForecastService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<TrendAnalysis> GenerateTrendAnalysisAsync(string userId, int historicalDays = 90, int forecastDays = 30)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-historicalDays);

        var analysis = new TrendAnalysis
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            HistoricalDays = historicalDays,
            ForecastDays = forecastDays
        };

        // Get all events for the period
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate.AddDays(1), userId);

        // Generate trends for key metrics
        analysis.Trends.Add(await GenerateMetricTrendAsync(userId, TrendMetric.TotalEvents, historicalDays, events));
        analysis.Trends.Add(await GenerateMetricTrendAsync(userId, TrendMetric.MeetingCount, historicalDays, events));
        analysis.Trends.Add(await GenerateMetricTrendAsync(userId, TrendMetric.MeetingCost, historicalDays, events));
        analysis.Trends.Add(await GenerateMetricTrendAsync(userId, TrendMetric.ScheduledHours, historicalDays, events));
        analysis.Trends.Add(await GenerateMetricTrendAsync(userId, TrendMetric.FocusTimeHours, historicalDays, events));

        // Generate predictions
        analysis.Predictions.Add(await GenerateMetricPredictionAsync(userId, TrendMetric.TotalEvents, historicalDays, forecastDays, events));
        analysis.Predictions.Add(await GenerateMetricPredictionAsync(userId, TrendMetric.MeetingCost, historicalDays, forecastDays, events));
        analysis.Predictions.Add(await GenerateMetricPredictionAsync(userId, TrendMetric.ScheduledHours, historicalDays, forecastDays, events));

        // Detect patterns
        analysis.DetectedPatterns = await DetectPatternsAsync(userId, historicalDays);

        // Generate key insights
        analysis.KeyInsights = GenerateKeyInsights(analysis);

        // Determine workload pattern
        analysis.WorkloadPattern = DetermineWorkloadPattern(analysis);

        // Generate warnings
        analysis.Warnings = GenerateWarnings(analysis);

        return analysis;
    }

    public async Task<MetricTrend> GetMetricTrendAsync(string userId, TrendMetric metric, int days = 90)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate.AddDays(1), userId);

        return await GenerateMetricTrendAsync(userId, metric, days, events);
    }

    public async Task<MetricPrediction> GetMetricPredictionAsync(string userId, TrendMetric metric, int historicalDays = 90, int forecastDays = 30)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-historicalDays);
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate.AddDays(1), userId);

        return await GenerateMetricPredictionAsync(userId, metric, historicalDays, forecastDays, events);
    }

    public async Task<List<WorkloadForecast>> ForecastWorkloadAsync(string userId, int forecastDays = 30)
    {
        var historicalDays = 90;
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-historicalDays);
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate.AddDays(1), userId);

        var forecasts = new List<WorkloadForecast>();
        var weeklyData = CalculateWeeklyAverages(events, startDate, endDate);

        // Forecast for next 4 weeks
        var weeksToForecast = (forecastDays + 6) / 7;
        for (int week = 0; week < weeksToForecast; week++)
        {
            var weekStart = endDate.AddDays(week * 7 + 1);
            var weekEnd = weekStart.AddDays(6);

            var avgEvents = weeklyData.avgEvents;
            var avgHours = weeklyData.avgHours;
            var avgCost = weeklyData.avgCost;

            forecasts.Add(new WorkloadForecast
            {
                StartDate = weekStart,
                EndDate = weekEnd,
                PredictedEventCount = (int)Math.Round(avgEvents),
                PredictedHours = Math.Round(avgHours, 1),
                PredictedMeetingCost = Math.Round(avgCost, 2),
                WorkloadLevel = DetermineWorkloadLevel(avgHours),
                Description = $"Week of {weekStart:MMM dd}"
            });
        }

        return forecasts;
    }

    public async Task<List<string>> DetectPatternsAsync(string userId, int days = 90)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate.AddDays(1), userId);

        var patterns = new List<string>();

        // Analyze day-of-week patterns
        var eventsByDay = events.GroupBy(e => e.StartTime.DayOfWeek)
            .Select(g => new { Day = g.Key, Count = g.Count(), Hours = g.Sum(e => (e.EndTime - e.StartTime).TotalHours) })
            .OrderByDescending(x => x.Count)
            .ToList();

        if (eventsByDay.Any())
        {
            var busiestDay = eventsByDay.First();
            patterns.Add($"Your busiest day is {busiestDay.Day} with an average of {(double)busiestDay.Count / (days / 7):F1} events");
        }

        // Analyze time-of-day patterns
        var morningEvents = events.Count(e => e.StartTime.Hour >= 6 && e.StartTime.Hour < 12);
        var afternoonEvents = events.Count(e => e.StartTime.Hour >= 12 && e.StartTime.Hour < 17);
        var eveningEvents = events.Count(e => e.StartTime.Hour >= 17 && e.StartTime.Hour < 22);

        var total = morningEvents + afternoonEvents + eveningEvents;
        if (total > 0)
        {
            var morningPct = (morningEvents * 100.0) / total;
            var afternoonPct = (afternoonEvents * 100.0) / total;
            var eveningPct = (eveningEvents * 100.0) / total;

            if (morningPct > 50)
                patterns.Add($"You're a morning person - {morningPct:F0}% of your events are scheduled before noon");
            else if (afternoonPct > 50)
                patterns.Add($"Your peak productivity is in the afternoon - {afternoonPct:F0}% of events");
            else if (eveningPct > 30)
                patterns.Add($"You schedule {eveningPct:F0}% of events in the evening - consider work-life balance");
        }

        // Analyze meeting patterns
        var meetings = events.Where(e => e.EventType == Core.Enums.EventType.Meeting).ToList();
        if (meetings.Any())
        {
            var avgMeetingDuration = meetings.Average(m => (m.EndTime - m.StartTime).TotalMinutes);
            patterns.Add($"Your average meeting duration is {avgMeetingDuration:F0} minutes");

            var meetingCount = meetings.Count;
            var weeklyMeetings = meetingCount / (days / 7.0);
            patterns.Add($"You average {weeklyMeetings:F1} meetings per week");
        }

        // Analyze workload trends
        var recentWeek = events.Count(e => e.StartTime >= endDate.AddDays(-7));
        var previousWeek = events.Count(e => e.StartTime >= endDate.AddDays(-14) && e.StartTime < endDate.AddDays(-7));

        if (previousWeek > 0)
        {
            var weekChange = ((recentWeek - previousWeek) * 100.0) / previousWeek;
            if (Math.Abs(weekChange) > 20)
            {
                if (weekChange > 0)
                    patterns.Add($"Your workload has increased by {weekChange:F0}% compared to last week");
                else
                    patterns.Add($"Your workload has decreased by {Math.Abs(weekChange):F0}% compared to last week");
            }
        }

        return patterns;
    }

    // Helper methods for trend generation
    private Task<MetricTrend> GenerateMetricTrendAsync(string userId, TrendMetric metric, int days, IEnumerable<Event> events)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);

        var trend = new MetricTrend
        {
            Metric = metric,
            MetricName = GetMetricName(metric)
        };

        // Group events by week for trending
        var weeklyData = new List<TrendDataPoint>();
        for (var date = startDate; date <= endDate; date = date.AddDays(7))
        {
            var weekEnd = date.AddDays(6);
            if (weekEnd > endDate) weekEnd = endDate;

            var weekEvents = events.Where(e => e.StartTime.Date >= date && e.StartTime.Date <= weekEnd).ToList();
            var value = CalculateMetricValue(metric, weekEvents);

            weeklyData.Add(new TrendDataPoint
            {
                Date = date,
                Value = value,
                Label = $"{date:MMM dd}"
            });
        }

        trend.HistoricalData = weeklyData;

        if (weeklyData.Any())
        {
            trend.Average = weeklyData.Average(d => d.Value);
            trend.Min = weeklyData.Min(d => d.Value);
            trend.Max = weeklyData.Max(d => d.Value);

            // Calculate trend direction using linear regression
            trend.Direction = CalculateTrendDirection(weeklyData);
            trend.ChangePercentage = CalculateChangePercentage(weeklyData);
            trend.Interpretation = GenerateTrendInterpretation(metric, trend.Direction, trend.ChangePercentage);
        }

        return Task.FromResult(trend);
    }

    private Task<MetricPrediction> GenerateMetricPredictionAsync(string userId, TrendMetric metric, int historicalDays, int forecastDays, IEnumerable<Event> events)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-historicalDays);

        var prediction = new MetricPrediction
        {
            Metric = metric,
            MetricName = GetMetricName(metric)
        };

        // Calculate weekly historical data
        var weeklyData = new List<TrendDataPoint>();
        for (var date = startDate; date <= endDate; date = date.AddDays(7))
        {
            var weekEnd = date.AddDays(6);
            if (weekEnd > endDate) weekEnd = endDate;

            var weekEvents = events.Where(e => e.StartTime.Date >= date && e.StartTime.Date <= weekEnd).ToList();
            var value = CalculateMetricValue(metric, weekEvents);

            weeklyData.Add(new TrendDataPoint
            {
                Date = date,
                Value = value
            });
        }

        if (weeklyData.Count > 0)
        {
            // Simple moving average prediction
            var recentValues = weeklyData.Skip(Math.Max(0, weeklyData.Count - 4)).Select(d => d.Value).ToList();
            var movingAverage = recentValues.Average();

            // Linear regression for trend
            var (slope, intercept) = CalculateLinearRegression(weeklyData);

            // Generate predictions
            var predictedData = new List<TrendDataPoint>();
            var weeksToForecast = (forecastDays + 6) / 7;

            for (int week = 1; week <= weeksToForecast; week++)
            {
                var futureDate = endDate.AddDays(week * 7);
                var x = weeklyData.Count + week;
                var trendValue = slope * x + intercept;

                // Blend moving average and trend (70% trend, 30% moving average)
                var predictedValue = (trendValue * 0.7) + (movingAverage * 0.3);
                predictedValue = Math.Max(0, predictedValue); // Ensure non-negative

                predictedData.Add(new TrendDataPoint
                {
                    Date = futureDate,
                    Value = Math.Round(predictedValue, 2),
                    Label = $"{futureDate:MMM dd}"
                });
            }

            prediction.PredictedData = predictedData;
            prediction.PredictedValue = predictedData.Any() ? predictedData.Average(d => d.Value) : 0;
            prediction.ConfidenceLevel = CalculateConfidence(weeklyData);
            prediction.Recommendation = GeneratePredictionRecommendation(metric, prediction.PredictedValue, movingAverage);
        }

        return Task.FromResult(prediction);
    }

    private double CalculateMetricValue(TrendMetric metric, List<Event> events)
    {
        return metric switch
        {
            TrendMetric.TotalEvents => events.Count,
            TrendMetric.MeetingCount => events.Count(e => e.EventType == Core.Enums.EventType.Meeting),
            TrendMetric.MeetingCost => (double)events.Where(e => e.EventType == Core.Enums.EventType.Meeting).Sum(e => e.MeetingCost ?? 0),
            TrendMetric.ScheduledHours => events.Sum(e => (e.EndTime - e.StartTime).TotalHours),
            TrendMetric.FocusTimeHours => events.Where(e => e.Title.Contains("Focus", StringComparison.OrdinalIgnoreCase) ||
                                                             e.Title.Contains("Deep Work", StringComparison.OrdinalIgnoreCase))
                                                .Sum(e => (e.EndTime - e.StartTime).TotalHours),
            TrendMetric.BackToBackMeetings => CalculateBackToBackMeetings(events),
            _ => 0
        };
    }

    private int CalculateBackToBackMeetings(List<Event> events)
    {
        var meetings = events.Where(e => e.EventType == Core.Enums.EventType.Meeting)
            .OrderBy(e => e.StartTime)
            .ToList();

        int count = 0;
        for (int i = 0; i < meetings.Count - 1; i++)
        {
            if (meetings[i].EndTime == meetings[i + 1].StartTime ||
                (meetings[i + 1].StartTime - meetings[i].EndTime).TotalMinutes <= 15)
            {
                count++;
            }
        }
        return count;
    }

    private TrendDirection CalculateTrendDirection(List<TrendDataPoint> data)
    {
        if (data.Count < 3) return TrendDirection.Stable;

        var (slope, _) = CalculateLinearRegression(data);
        var avgValue = data.Average(d => d.Value);

        if (avgValue == 0) return TrendDirection.Stable;

        var slopePercentage = (slope / avgValue) * 100;

        // Calculate volatility
        var variance = data.Select(d => Math.Pow(d.Value - avgValue, 2)).Average();
        var stdDev = Math.Sqrt(variance);
        var coefficientOfVariation = (stdDev / avgValue) * 100;

        if (coefficientOfVariation > 30)
            return TrendDirection.Volatile;
        else if (slopePercentage > 5)
            return TrendDirection.Increasing;
        else if (slopePercentage < -5)
            return TrendDirection.Decreasing;
        else
            return TrendDirection.Stable;
    }

    private double CalculateChangePercentage(List<TrendDataPoint> data)
    {
        if (data.Count < 2) return 0;

        var firstValue = data.First().Value;
        var lastValue = data.Last().Value;

        if (firstValue == 0) return 0;

        return ((lastValue - firstValue) / firstValue) * 100;
    }

    private (double slope, double intercept) CalculateLinearRegression(List<TrendDataPoint> data)
    {
        if (data.Count < 2) return (0, 0);

        int n = data.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        for (int i = 0; i < n; i++)
        {
            double x = i;
            double y = data[i].Value;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        double intercept = (sumY - slope * sumX) / n;

        return (slope, intercept);
    }

    private double CalculateConfidence(List<TrendDataPoint> data)
    {
        if (data.Count < 4) return 50;

        var avg = data.Average(d => d.Value);
        var variance = data.Select(d => Math.Pow(d.Value - avg, 2)).Average();
        var stdDev = Math.Sqrt(variance);
        var coefficientOfVariation = avg > 0 ? (stdDev / avg) * 100 : 100;

        // Lower CV = higher confidence
        if (coefficientOfVariation < 15) return 90;
        if (coefficientOfVariation < 25) return 75;
        if (coefficientOfVariation < 40) return 60;
        return 50;
    }

    private (double avgEvents, double avgHours, double avgCost) CalculateWeeklyAverages(IEnumerable<Event> events, DateTime startDate, DateTime endDate)
    {
        var totalWeeks = ((endDate - startDate).Days + 1) / 7.0;
        if (totalWeeks == 0) return (0, 0, 0);

        var totalEvents = events.Count();
        var totalHours = events.Sum(e => (e.EndTime - e.StartTime).TotalHours);
        var totalCost = events.Where(e => e.EventType == Core.Enums.EventType.Meeting).Sum(e => e.MeetingCost ?? 0);

        return (totalEvents / totalWeeks, totalHours / totalWeeks, (double)totalCost / totalWeeks);
    }

    private string DetermineWorkloadLevel(double hours)
    {
        return hours switch
        {
            <= 20 => "Light",
            <= 35 => "Moderate",
            <= 45 => "Heavy",
            _ => "Very Heavy"
        };
    }

    private string GetMetricName(TrendMetric metric)
    {
        return metric switch
        {
            TrendMetric.TotalEvents => "Total Events",
            TrendMetric.MeetingCount => "Meeting Count",
            TrendMetric.MeetingCost => "Meeting Cost ($)",
            TrendMetric.ScheduledHours => "Scheduled Hours",
            TrendMetric.FocusTimeHours => "Focus Time (Hours)",
            TrendMetric.CalendarHealthScore => "Calendar Health Score",
            TrendMetric.TaskCompletionRate => "Task Completion Rate (%)",
            TrendMetric.BackToBackMeetings => "Back-to-Back Meetings",
            _ => metric.ToString()
        };
    }

    private string GenerateTrendInterpretation(TrendMetric metric, TrendDirection direction, double changePercentage)
    {
        var directionText = direction switch
        {
            TrendDirection.Increasing => $"increasing by {Math.Abs(changePercentage):F1}%",
            TrendDirection.Decreasing => $"decreasing by {Math.Abs(changePercentage):F1}%",
            TrendDirection.Volatile => "showing volatile patterns",
            _ => "remaining stable"
        };

        return $"Your {GetMetricName(metric).ToLower()} is {directionText} over the analyzed period.";
    }

    private string GeneratePredictionRecommendation(TrendMetric metric, double predictedValue, double currentAverage)
    {
        var change = ((predictedValue - currentAverage) / currentAverage) * 100;

        return metric switch
        {
            TrendMetric.MeetingCost when change > 20 =>
                "Meeting costs are trending up significantly. Consider reviewing meeting efficiency and necessity.",
            TrendMetric.MeetingCount when change > 15 =>
                "Meeting frequency is increasing. Ensure meetings are adding value and consider consolidation.",
            TrendMetric.ScheduledHours when predictedValue > 45 =>
                "Workload is trending toward overcommitment. Consider blocking time for breaks and focused work.",
            TrendMetric.TotalEvents when change > 20 =>
                "Calendar density is increasing rapidly. Monitor for signs of burnout and maintain work-life balance.",
            _ => "Trends are within normal ranges. Continue monitoring for changes."
        };
    }

    private string DetermineWorkloadPattern(TrendAnalysis analysis)
    {
        var eventTrend = analysis.Trends.FirstOrDefault(t => t.Metric == TrendMetric.TotalEvents);
        if (eventTrend == null) return "Insufficient data";

        return eventTrend.Direction switch
        {
            TrendDirection.Increasing => "Workload Increasing - Growing calendar commitments",
            TrendDirection.Decreasing => "Workload Decreasing - Calendar becoming lighter",
            TrendDirection.Volatile => "Volatile Workload - Inconsistent scheduling patterns",
            _ => "Stable Workload - Consistent scheduling"
        };
    }

    private List<string> GenerateKeyInsights(TrendAnalysis analysis)
    {
        var insights = new List<string>();

        // Analyze meeting cost trends
        var costTrend = analysis.Trends.FirstOrDefault(t => t.Metric == TrendMetric.MeetingCost);
        if (costTrend != null && costTrend.Direction == TrendDirection.Increasing && Math.Abs(costTrend.ChangePercentage) > 20)
        {
            insights.Add($"Meeting costs have increased by {Math.Abs(costTrend.ChangePercentage):F0}% - consider optimizing meeting efficiency");
        }

        // Analyze scheduled hours
        var hoursTrend = analysis.Trends.FirstOrDefault(t => t.Metric == TrendMetric.ScheduledHours);
        if (hoursTrend != null && hoursTrend.Average > 50)
        {
            insights.Add($"Average weekly scheduled time is {hoursTrend.Average:F0} hours - monitor for potential overcommitment");
        }

        // Analyze focus time
        var focusTrend = analysis.Trends.FirstOrDefault(t => t.Metric == TrendMetric.FocusTimeHours);
        if (focusTrend != null && focusTrend.Average < 10)
        {
            insights.Add("Limited focus time detected - consider blocking dedicated time for deep work");
        }

        // Look at predictions
        var costPrediction = analysis.Predictions.FirstOrDefault(p => p.Metric == TrendMetric.MeetingCost);
        if (costPrediction != null && costPrediction.PredictedValue > costTrend?.Average * 1.3)
        {
            insights.Add("Meeting costs are forecasted to increase significantly in coming weeks");
        }

        if (!insights.Any())
        {
            insights.Add("Your calendar trends are healthy and within optimal ranges");
        }

        return insights;
    }

    private List<string> GenerateWarnings(TrendAnalysis analysis)
    {
        var warnings = new List<string>();

        // Check for rapidly increasing workload
        var eventTrend = analysis.Trends.FirstOrDefault(t => t.Metric == TrendMetric.TotalEvents);
        if (eventTrend != null && eventTrend.Direction == TrendDirection.Increasing && Math.Abs(eventTrend.ChangePercentage) > 30)
        {
            warnings.Add("⚠️ Rapid workload increase detected - risk of burnout");
        }

        // Check for excessive meeting load
        var meetingTrend = analysis.Trends.FirstOrDefault(t => t.Metric == TrendMetric.MeetingCount);
        if (meetingTrend != null && meetingTrend.Average > 20)
        {
            warnings.Add("⚠️ High meeting frequency may impact productivity");
        }

        // Check predictions for concerning trends
        var hoursPrediction = analysis.Predictions.FirstOrDefault(p => p.Metric == TrendMetric.ScheduledHours);
        if (hoursPrediction != null && hoursPrediction.PredictedValue > 50)
        {
            warnings.Add("⚠️ Forecasted workload exceeds healthy limits");
        }

        return warnings;
    }
}

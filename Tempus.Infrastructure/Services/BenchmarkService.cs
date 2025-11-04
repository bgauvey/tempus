using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Enums;

namespace Tempus.Infrastructure.Services;

public class BenchmarkService : IBenchmarkService
{
    private readonly IAnalyticsService _analyticsService;
    private readonly BenchmarkData _industryBenchmarks;

    public BenchmarkService(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
        _industryBenchmarks = new BenchmarkData();
    }

    public BenchmarkData GetIndustryBenchmarks()
    {
        return _industryBenchmarks;
    }

    public async Task<BenchmarkReport> GenerateBenchmarkReportAsync(string userId, int periodDays = 30)
    {
        // Get analytics data
        var analytics = await _analyticsService.GetQuickStatsAsync(userId, periodDays);
        var comparisons = new List<BenchmarkComparison>();

        // Meeting-related comparisons
        comparisons.Add(CompareMeetingPercentage(analytics));
        comparisons.Add(CompareAverageMeetingDuration(analytics));
        comparisons.Add(CompareAverageMeetingAttendees(analytics));

        // Focus time comparisons
        comparisons.Add(CompareFocusTime(analytics));
        comparisons.Add(CompareFocusBlockDuration(analytics));

        // Work hours comparisons
        comparisons.Add(CompareWeeklyHours(analytics, periodDays));
        comparisons.Add(CompareDailyHours(analytics, periodDays));

        // Task management comparisons
        comparisons.Add(CompareTaskCompletionRate(analytics));

        // Schedule fragmentation
        comparisons.Add(CompareFragmentation(analytics));

        // Calculate overall score
        var overallScore = CalculateOverallScore(comparisons);

        // Count metrics by status
        var metricsAtStandard = comparisons.Count(c => c.Status == BenchmarkStatus.AtStandard || c.Status == BenchmarkStatus.NearStandard);
        var metricsAboveStandard = comparisons.Count(c => c.Status == BenchmarkStatus.Excellent);
        var metricsBelowStandard = comparisons.Count(c => c.Status == BenchmarkStatus.BelowStandard);

        // Generate top recommendations
        var topRecommendations = GenerateTopRecommendations(comparisons);

        return new BenchmarkReport
        {
            GeneratedDate = DateTime.UtcNow,
            AnalysisPeriodDays = periodDays,
            Comparisons = comparisons,
            OverallScore = overallScore,
            MetricsAtStandard = metricsAtStandard,
            MetricsAboveStandard = metricsAboveStandard,
            MetricsBelowStandard = metricsBelowStandard,
            TopRecommendations = topRecommendations
        };
    }

    private BenchmarkComparison CompareMeetingPercentage(CalendarAnalytics analytics)
    {
        var totalHours = analytics.TimeUsage.TotalScheduledHours;
        var meetingHours = analytics.MeetingCount > 0
            ? analytics.MeetingStats.TotalMeetingHours
            : 0;

        var actualPercentage = totalHours > 0 ? ((double)meetingHours / totalHours) * 100 : 0;
        var benchmarkValue = _industryBenchmarks.OptimalMeetingPercentage;

        var status = DetermineStatus(actualPercentage, benchmarkValue, _industryBenchmarks.MaxMeetingPercentage, lowerIsBetter: false);

        return new BenchmarkComparison
        {
            Category = "Meetings",
            MetricName = "Meeting Time Percentage",
            ActualValue = actualPercentage,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualPercentage, benchmarkValue),
            Status = status,
            Description = $"You spend {actualPercentage:F1}% of your time in meetings. Industry standard is {benchmarkValue:F0}%.",
            Recommendation = GetMeetingPercentageRecommendation(actualPercentage, benchmarkValue),
            Icon = "groups"
        };
    }

    private BenchmarkComparison CompareAverageMeetingDuration(CalendarAnalytics analytics)
    {
        var actualMinutes = analytics.MeetingStats?.AverageMeetingDuration ?? 0;
        var benchmarkValue = _industryBenchmarks.OptimalMeetingDurationMinutes;

        var status = DetermineStatus(actualMinutes, benchmarkValue, _industryBenchmarks.MaxMeetingDurationMinutes, lowerIsBetter: true);

        return new BenchmarkComparison
        {
            Category = "Meetings",
            MetricName = "Average Meeting Duration",
            ActualValue = actualMinutes,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualMinutes, benchmarkValue),
            Status = status,
            Description = $"Your average meeting lasts {actualMinutes:F0} minutes. Optimal is {benchmarkValue} minutes.",
            Recommendation = GetMeetingDurationRecommendation(actualMinutes, benchmarkValue),
            Icon = "schedule"
        };
    }

    private BenchmarkComparison CompareAverageMeetingAttendees(CalendarAnalytics analytics)
    {
        // Calculate average attendees from top attendees data
        var actualAttendees = analytics.MeetingStats?.TopAttendees?.Average(a => a.MeetingCount) ?? 2.0;
        var benchmarkValue = _industryBenchmarks.OptimalMeetingAttendees;

        var status = DetermineStatus(actualAttendees, benchmarkValue, _industryBenchmarks.MaxMeetingAttendees, lowerIsBetter: true);

        return new BenchmarkComparison
        {
            Category = "Meetings",
            MetricName = "Average Meeting Size",
            ActualValue = actualAttendees,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualAttendees, benchmarkValue),
            Status = status,
            Description = $"Your meetings average {actualAttendees:F1} attendees. Optimal group size is {benchmarkValue}.",
            Recommendation = GetMeetingSizeRecommendation(actualAttendees, benchmarkValue),
            Icon = "people"
        };
    }

    private BenchmarkComparison CompareFocusTime(CalendarAnalytics analytics)
    {
        var focusHours = analytics.Productivity?.TotalFocusHours ?? 0;
        var totalHours = analytics.TimeUsage.TotalScheduledHours;
        var actualPercentage = totalHours > 0 ? (focusHours / totalHours) * 100 : 0;
        var benchmarkValue = _industryBenchmarks.OptimalFocusTimePercentage;

        var status = actualPercentage >= benchmarkValue ? BenchmarkStatus.Excellent :
                     actualPercentage >= _industryBenchmarks.MinFocusTimePercentage ? BenchmarkStatus.AtStandard :
                     actualPercentage >= _industryBenchmarks.MinFocusTimePercentage * 0.8 ? BenchmarkStatus.NearStandard :
                     BenchmarkStatus.BelowStandard;

        return new BenchmarkComparison
        {
            Category = "Focus Time",
            MetricName = "Focus Time Percentage",
            ActualValue = actualPercentage,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualPercentage, benchmarkValue),
            Status = status,
            Description = $"You spend {actualPercentage:F1}% of your time in focused work. Optimal is {benchmarkValue:F0}%.",
            Recommendation = GetFocusTimeRecommendation(actualPercentage, benchmarkValue),
            Icon = "psychology"
        };
    }

    private BenchmarkComparison CompareFocusBlockDuration(CalendarAnalytics analytics)
    {
        // Calculate average focus block duration from TimeBlock events
        var focusHours = analytics.Productivity?.TotalFocusHours ?? 0;
        var focusBlocks = analytics.Productivity?.FocusTimeBlocks ?? 1;
        var actualMinutes = focusBlocks > 0 ? (focusHours / focusBlocks) * 60 : 0;
        var benchmarkValue = _industryBenchmarks.OptimalFocusBlockMinutes;

        var status = actualMinutes >= benchmarkValue ? BenchmarkStatus.Excellent :
                     actualMinutes >= _industryBenchmarks.MinFocusBlockMinutes ? BenchmarkStatus.AtStandard :
                     actualMinutes >= _industryBenchmarks.MinFocusBlockMinutes * 0.8 ? BenchmarkStatus.NearStandard :
                     BenchmarkStatus.BelowStandard;

        return new BenchmarkComparison
        {
            Category = "Focus Time",
            MetricName = "Average Focus Block Duration",
            ActualValue = actualMinutes,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualMinutes, benchmarkValue),
            Status = status,
            Description = $"Your focus blocks average {actualMinutes:F0} minutes. Optimal is {benchmarkValue} minutes.",
            Recommendation = GetFocusBlockRecommendation(actualMinutes, benchmarkValue),
            Icon = "timer"
        };
    }

    private BenchmarkComparison CompareWeeklyHours(CalendarAnalytics analytics, int periodDays)
    {
        var daysInPeriod = periodDays;
        var weeksInPeriod = daysInPeriod / 7.0;
        var actualWeeklyHours = weeksInPeriod > 0 ? analytics.TimeUsage.TotalScheduledHours / weeksInPeriod : 0;
        var benchmarkValue = _industryBenchmarks.OptimalWeeklyHours;

        var status = actualWeeklyHours <= _industryBenchmarks.MaxHealthyWeeklyHours && actualWeeklyHours >= benchmarkValue * 0.9
            ? BenchmarkStatus.AtStandard :
                     actualWeeklyHours > _industryBenchmarks.MaxHealthyWeeklyHours ? BenchmarkStatus.BelowStandard :
                     BenchmarkStatus.NearStandard;

        return new BenchmarkComparison
        {
            Category = "Work Hours",
            MetricName = "Weekly Work Hours",
            ActualValue = actualWeeklyHours,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualWeeklyHours, benchmarkValue),
            Status = status,
            Description = $"You average {actualWeeklyHours:F1} hours per week. Standard is {benchmarkValue} hours.",
            Recommendation = GetWeeklyHoursRecommendation(actualWeeklyHours, benchmarkValue),
            Icon = "today"
        };
    }

    private BenchmarkComparison CompareDailyHours(CalendarAnalytics analytics, int periodDays)
    {
        var actualDailyHours = periodDays > 0 ? (double)analytics.TimeUsage.TotalScheduledHours / periodDays : 0;
        var benchmarkValue = _industryBenchmarks.OptimalWeeklyHours / 5.0; // Daily average

        var status = actualDailyHours <= _industryBenchmarks.MaxDailyHours && actualDailyHours >= _industryBenchmarks.MinDailyHours
            ? BenchmarkStatus.AtStandard :
                     actualDailyHours > _industryBenchmarks.MaxDailyHours ? BenchmarkStatus.BelowStandard :
                     BenchmarkStatus.NearStandard;

        return new BenchmarkComparison
        {
            Category = "Work Hours",
            MetricName = "Daily Work Hours",
            ActualValue = actualDailyHours,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualDailyHours, benchmarkValue),
            Status = status,
            Description = $"You average {actualDailyHours:F1} hours per day. Standard is {benchmarkValue:F1} hours.",
            Recommendation = GetDailyHoursRecommendation(actualDailyHours, benchmarkValue),
            Icon = "event"
        };
    }

    private BenchmarkComparison CompareTaskCompletionRate(CalendarAnalytics analytics)
    {
        var actualRate = analytics.Productivity?.TaskCompletionRate ?? 0;
        var benchmarkValue = _industryBenchmarks.OptimalTaskCompletionRate;

        var status = actualRate >= benchmarkValue ? BenchmarkStatus.Excellent :
                     actualRate >= benchmarkValue * 0.9 ? BenchmarkStatus.AtStandard :
                     actualRate >= benchmarkValue * 0.7 ? BenchmarkStatus.NearStandard :
                     BenchmarkStatus.BelowStandard;

        return new BenchmarkComparison
        {
            Category = "Task Management",
            MetricName = "Task Completion Rate",
            ActualValue = actualRate,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(actualRate, benchmarkValue),
            Status = status,
            Description = $"You complete {actualRate:F1}% of your tasks. Optimal rate is {benchmarkValue:F0}%.",
            Recommendation = GetTaskCompletionRecommendation(actualRate, benchmarkValue),
            Icon = "check_circle"
        };
    }

    private BenchmarkComparison CompareFragmentation(CalendarAnalytics analytics)
    {
        // Use fragmented hours as a proxy for fragmentation score
        var actualFragmentation = analytics.Productivity?.FragmentedHours ?? 0;
        var totalHours = analytics.TimeUsage.TotalScheduledHours;
        var fragmentationScore = totalHours > 0 ? ((double)actualFragmentation / totalHours) * 10 : 0;
        var benchmarkValue = _industryBenchmarks.MaxFragmentationScore;

        var status = fragmentationScore <= benchmarkValue ? BenchmarkStatus.Excellent :
                     fragmentationScore <= benchmarkValue * 1.2 ? BenchmarkStatus.AtStandard :
                     fragmentationScore <= benchmarkValue * 1.5 ? BenchmarkStatus.NearStandard :
                     BenchmarkStatus.BelowStandard;

        return new BenchmarkComparison
        {
            Category = "Schedule Quality",
            MetricName = "Schedule Fragmentation",
            ActualValue = fragmentationScore,
            BenchmarkValue = benchmarkValue,
            Variance = CalculateVariance(fragmentationScore, benchmarkValue),
            Status = status,
            Description = $"Your schedule fragmentation score is {fragmentationScore:F1}. Lower is better (target: ≤{benchmarkValue:F1}).",
            Recommendation = GetFragmentationRecommendation(fragmentationScore, benchmarkValue),
            Icon = "auto_fix_high"
        };
    }

    private BenchmarkStatus DetermineStatus(double actualValue, double benchmarkValue, double maxValue, bool lowerIsBetter)
    {
        if (lowerIsBetter)
        {
            if (actualValue <= benchmarkValue) return BenchmarkStatus.Excellent;
            if (actualValue <= maxValue) return BenchmarkStatus.AtStandard;
            if (actualValue <= maxValue * 1.2) return BenchmarkStatus.NearStandard;
            return BenchmarkStatus.BelowStandard;
        }
        else
        {
            if (actualValue >= benchmarkValue) return BenchmarkStatus.Excellent;
            if (actualValue >= benchmarkValue * 0.9) return BenchmarkStatus.AtStandard;
            if (actualValue >= benchmarkValue * 0.7) return BenchmarkStatus.NearStandard;
            return BenchmarkStatus.BelowStandard;
        }
    }

    private double CalculateVariance(double actualValue, double benchmarkValue)
    {
        if (benchmarkValue == 0) return 0;
        return ((actualValue - benchmarkValue) / benchmarkValue) * 100;
    }

    public BenchmarkComparison CompareMetric(string category, string metricName, double actualValue, double benchmarkValue, bool higherIsBetter = true)
    {
        var variance = CalculateVariance(actualValue, benchmarkValue);
        var status = higherIsBetter
            ? (actualValue >= benchmarkValue ? BenchmarkStatus.Excellent : BenchmarkStatus.BelowStandard)
            : (actualValue <= benchmarkValue ? BenchmarkStatus.Excellent : BenchmarkStatus.BelowStandard);

        return new BenchmarkComparison
        {
            Category = category,
            MetricName = metricName,
            ActualValue = actualValue,
            BenchmarkValue = benchmarkValue,
            Variance = variance,
            Status = status,
            Description = $"Your {metricName.ToLower()} is {actualValue:F1}. Benchmark is {benchmarkValue:F1}.",
            Recommendation = variance > 0 ? "You're above the benchmark." : "Consider improving this metric.",
            Icon = "analytics"
        };
    }

    public double CalculateOverallScore(List<BenchmarkComparison> comparisons)
    {
        if (!comparisons.Any()) return 0;

        var score = 0.0;
        foreach (var comparison in comparisons)
        {
            score += comparison.Status switch
            {
                BenchmarkStatus.Excellent => 100,
                BenchmarkStatus.AtStandard => 85,
                BenchmarkStatus.NearStandard => 70,
                BenchmarkStatus.BelowStandard => 50,
                _ => 0
            };
        }

        return score / comparisons.Count;
    }

    private List<string> GenerateTopRecommendations(List<BenchmarkComparison> comparisons)
    {
        var recommendations = new List<string>();

        // Get the worst performing metrics
        var belowStandard = comparisons
            .Where(c => c.Status == BenchmarkStatus.BelowStandard)
            .OrderBy(c => c.Variance)
            .Take(3)
            .ToList();

        foreach (var metric in belowStandard)
        {
            recommendations.Add(metric.Recommendation);
        }

        // If we have fewer than 3, add some general recommendations
        if (recommendations.Count < 3)
        {
            var nearStandard = comparisons
                .Where(c => c.Status == BenchmarkStatus.NearStandard)
                .OrderBy(c => c.Variance)
                .Take(3 - recommendations.Count)
                .ToList();

            foreach (var metric in nearStandard)
            {
                recommendations.Add(metric.Recommendation);
            }
        }

        return recommendations.Take(3).ToList();
    }

    // Recommendation methods
    private string GetMeetingPercentageRecommendation(double actual, double benchmark)
    {
        if (actual > benchmark * 1.2)
            return "Consider declining unnecessary meetings or consolidating similar meetings to reduce meeting overload.";
        if (actual < benchmark * 0.7)
            return "You may want to increase collaboration time. Schedule more team sync meetings or brainstorming sessions.";
        return "Your meeting time is well-balanced. Continue monitoring to maintain this equilibrium.";
    }

    private string GetMeetingDurationRecommendation(double actual, double benchmark)
    {
        if (actual > benchmark * 1.5)
            return "Long meetings reduce engagement. Try breaking meetings into shorter focused sessions with clear agendas.";
        if (actual < benchmark * 0.5)
            return "Very short meetings may not allow for adequate discussion. Consider if these could be emails or async updates.";
        return "Your meeting durations are effective. Keep meetings focused and time-boxed.";
    }

    private string GetMeetingSizeRecommendation(double actual, double benchmark)
    {
        if (actual > benchmark * 1.5)
            return "Large meetings often lack productivity. Invite only key stakeholders and share notes with others.";
        if (actual < benchmark * 0.5)
            return "Very small meetings are efficient but ensure you're not excluding valuable perspectives.";
        return "Your meeting sizes are optimal for decision-making and collaboration.";
    }

    private string GetFocusTimeRecommendation(double actual, double benchmark)
    {
        if (actual < benchmark * 0.7)
            return "Protect more time for deep work. Block 2-4 hour chunks for focused tasks and mark them as 'Do Not Disturb'.";
        if (actual > benchmark * 1.3)
            return "Excellent focus time allocation! Make sure you're also dedicating time to collaboration and communication.";
        return "Your focus time is well-balanced. Continue scheduling uninterrupted blocks for deep work.";
    }

    private string GetFocusBlockRecommendation(double actual, double benchmark)
    {
        if (actual < benchmark * 0.7)
            return "Increase focus block duration to 90-120 minutes minimum. It takes time to enter deep work state.";
        return "Your focus blocks are long enough for meaningful deep work. Maintain these productive sessions.";
    }

    private string GetWeeklyHoursRecommendation(double actual, double benchmark)
    {
        if (actual > 50)
            return "You're working excessive hours, which increases burnout risk. Prioritize ruthlessly and delegate when possible.";
        if (actual < 30)
            return "Your calendar shows fewer hours than typical. Ensure you're capturing all work activities.";
        return "Your work hours are sustainable. Continue maintaining this healthy work-life balance.";
    }

    private string GetDailyHoursRecommendation(double actual, double benchmark)
    {
        if (actual > 10)
            return "Very long workdays lead to diminishing returns. Spread work across more days or reduce scope.";
        if (actual < 5)
            return "Your daily work hours are quite low. This may be intentional or you may need to block more work time.";
        return "Your daily work hours are sustainable and productive.";
    }

    private string GetTaskCompletionRecommendation(double actual, double benchmark)
    {
        if (actual < benchmark * 0.7)
            return "Low completion rate suggests overcommitment. Reduce concurrent tasks and focus on finishing what you start.";
        if (actual > benchmark * 1.1)
            return "Excellent completion rate! You're effectively managing your task load and follow-through.";
        return "Good task completion rate. Continue prioritizing and breaking large tasks into manageable pieces.";
    }

    private string GetFragmentationRecommendation(double actual, double benchmark)
    {
        if (actual > benchmark * 1.5)
            return "High fragmentation indicates too many context switches. Batch similar activities and add buffers between meetings.";
        return "Your schedule has good flow with manageable context switching. Maintain buffer time between different activities.";
    }
}

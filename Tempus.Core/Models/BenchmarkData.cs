namespace Tempus.Core.Models;

/// <summary>
/// Represents industry benchmark standards for calendar and time management
/// </summary>
public class BenchmarkData
{
    // Meeting-related benchmarks
    public double OptimalMeetingPercentage { get; set; } = 35.0; // 35% of work time
    public double MaxMeetingPercentage { get; set; } = 50.0; // Should not exceed 50%
    public int OptimalMeetingDurationMinutes { get; set; } = 30; // Most effective meeting length
    public int MaxMeetingDurationMinutes { get; set; } = 60; // Meetings over 60 min lose effectiveness
    public int OptimalMeetingAttendees { get; set; } = 7; // Optimal decision-making group size
    public int MaxMeetingAttendees { get; set; } = 12; // Beyond this, productivity drops

    // Focus time benchmarks
    public double MinFocusTimePercentage { get; set; } = 20.0; // Minimum 20% for deep work
    public double OptimalFocusTimePercentage { get; set; } = 40.0; // Ideal 40% for deep work
    public int MinFocusBlockMinutes { get; set; } = 90; // Minimum effective focus block
    public int OptimalFocusBlockMinutes { get; set; } = 120; // Optimal focus block (2 hours)

    // Work hours benchmarks
    public int OptimalWeeklyHours { get; set; } = 40; // Standard work week
    public int MaxHealthyWeeklyHours { get; set; } = 50; // Beyond this, burnout risk increases
    public int MinDailyHours { get; set; } = 6; // Minimum for full-time work
    public int MaxDailyHours { get; set; } = 10; // Beyond this, diminishing returns

    // Task management benchmarks
    public double OptimalTaskCompletionRate { get; set; } = 80.0; // 80% completion is healthy
    public int MaxActiveTasks { get; set; } = 7; // More than 7 tasks reduces focus

    // Schedule fragmentation benchmarks
    public double MaxFragmentationScore { get; set; } = 3.0; // Context switching threshold
    public int MinBufferBetweenMeetingsMinutes { get; set; } = 15; // Recommended buffer time

    // Work-life balance benchmarks
    public int MaxWorkDaysPerWeek { get; set; } = 5; // Standard work week
    public double MaxOvertimePercentage { get; set; } = 10.0; // Overtime should be < 10% of work time
}

/// <summary>
/// Represents a comparison result between actual metrics and benchmark standards
/// </summary>
public class BenchmarkComparison
{
    public string Category { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double ActualValue { get; set; }
    public double BenchmarkValue { get; set; }
    public double Variance { get; set; } // Percentage difference
    public BenchmarkStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Represents the overall benchmarking report
/// </summary>
public class BenchmarkReport
{
    public DateTime GeneratedDate { get; set; }
    public int AnalysisPeriodDays { get; set; }
    public List<BenchmarkComparison> Comparisons { get; set; } = new();
    public double OverallScore { get; set; } // 0-100 score
    public int MetricsAtStandard { get; set; }
    public int MetricsAboveStandard { get; set; }
    public int MetricsBelowStandard { get; set; }
    public List<string> TopRecommendations { get; set; } = new();
}

public enum BenchmarkStatus
{
    BelowStandard,  // Significantly below benchmark
    NearStandard,   // Close to benchmark
    AtStandard,     // Meeting benchmark
    AboveStandard,  // Exceeding benchmark (could be good or bad depending on metric)
    Excellent       // Significantly better than benchmark
}

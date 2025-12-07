using Tempus.Core.Enums;

namespace Tempus.Core.Models;

/// <summary>
/// Team-level analytics aggregating data across all team members
/// </summary>
public class TeamAnalytics
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int MemberCount { get; set; }

    // Aggregated Team Metrics
    public TeamMetrics AggregatedMetrics { get; set; } = new();

    // Per-Member Analytics
    public List<MemberAnalyticsSummary> MemberAnalytics { get; set; } = new();

    // Team Collaboration Metrics
    public TeamCollaborationMetrics Collaboration { get; set; } = new();

    // Team Health & Productivity
    public TeamHealthMetrics Health { get; set; } = new();

    // Cost Analysis
    public TeamCostAnalysis CostAnalysis { get; set; } = new();
}

/// <summary>
/// Aggregated metrics for the entire team
/// </summary>
public class TeamMetrics
{
    // Total team statistics
    public int TotalEvents { get; set; }
    public int TotalMeetings { get; set; }
    public double TotalHours { get; set; }
    public decimal TotalMeetingCost { get; set; }

    // Averages per member
    public double AverageEventsPerMember { get; set; }
    public double AverageMeetingsPerMember { get; set; }
    public double AverageHoursPerMember { get; set; }
    public decimal AverageCostPerMember { get; set; }

    // Team-wide distributions
    public Dictionary<string, int> EventTypeDistribution { get; set; } = new();
    public Dictionary<DayOfWeek, int> BusiestDays { get; set; } = new();
    public Dictionary<int, int> BusiestHours { get; set; } = new();

    // Focus time
    public int TotalFocusHours { get; set; }
    public double AverageFocusHoursPerMember { get; set; }
}

/// <summary>
/// Summary of analytics for an individual team member
/// </summary>
public class MemberAnalyticsSummary
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TeamRole Role { get; set; }

    // Individual metrics
    public int EventCount { get; set; }
    public int MeetingCount { get; set; }
    public double TotalHours { get; set; }
    public decimal MeetingCost { get; set; }
    public double HealthScore { get; set; }
    public int FocusTimeHours { get; set; }

    // Comparison to team average
    public double EventsVsTeamAverage { get; set; } // Percentage
    public double MeetingsVsTeamAverage { get; set; }
    public double HoursVsTeamAverage { get; set; }
    public double HealthScoreVsTeamAverage { get; set; }

    // Status indicators
    public WorkloadStatus WorkloadStatus { get; set; }
    public List<string> Insights { get; set; } = new();
}

/// <summary>
/// Team collaboration and meeting patterns
/// </summary>
public class TeamCollaborationMetrics
{
    // Internal team meetings
    public int InternalMeetings { get; set; }
    public double InternalMeetingHours { get; set; }
    public decimal InternalMeetingCost { get; set; }

    // External meetings (with non-team members)
    public int ExternalMeetings { get; set; }
    public double ExternalMeetingHours { get; set; }
    public decimal ExternalMeetingCost { get; set; }

    // Meeting patterns
    public double AverageMeetingSize { get; set; }
    public int BackToBackMeetings { get; set; }
    public double MeetingFragmentation { get; set; } // % of time in small meetings

    // Collaboration graph
    public List<CollaborationPair> TopCollaborators { get; set; } = new();
    public List<string> IsolatedMembers { get; set; } = new(); // Members with few collaborations
}

/// <summary>
/// Represents collaboration between two team members
/// </summary>
public class CollaborationPair
{
    public string Member1 { get; set; } = string.Empty;
    public string Member2 { get; set; } = string.Empty;
    public int MeetingCount { get; set; }
    public double TotalHours { get; set; }
}

/// <summary>
/// Team health and productivity indicators
/// </summary>
public class TeamHealthMetrics
{
    // Overall team health
    public double OverallHealthScore { get; set; } // 0-100
    public double AverageHealthScore { get; set; } // Average of member scores
    public double HealthScoreVariance { get; set; } // Indicates balance

    // Workload distribution
    public WorkloadBalance Balance { get; set; } = new();
    public int OverloadedMembers { get; set; }
    public int UnderutilizedMembers { get; set; }
    public int OptimalMembers { get; set; }

    // Productivity indicators
    public double TeamProductivity { get; set; } // 0-100
    public int TotalFocusBlocks { get; set; }
    public double FocusTimeBalance { get; set; } // Standard deviation of focus time

    // Issues and recommendations
    public List<string> HealthIssues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Workload balance assessment
/// </summary>
public class WorkloadBalance
{
    public double BalanceScore { get; set; } // 0-100, higher = more balanced
    public double StandardDeviation { get; set; }
    public string Status { get; set; } = string.Empty; // Balanced, Moderate Imbalance, Severe Imbalance
    public List<string> Imbalances { get; set; } = new();
}

/// <summary>
/// Team cost analysis and budget tracking
/// </summary>
public class TeamCostAnalysis
{
    // Total costs
    public decimal TotalMeetingCost { get; set; }
    public decimal AverageCostPerMeeting { get; set; }
    public decimal CostPerMember { get; set; }

    // Cost breakdown
    public Dictionary<string, decimal> CostByMeetingType { get; set; } = new();
    public List<CostlyTeamMeeting> MostCostlyMeetings { get; set; } = new();

    // Cost trends
    public decimal PreviousPeriodCost { get; set; }
    public decimal CostChange { get; set; }
    public double CostChangePercentage { get; set; }

    // Efficiency metrics
    public decimal CostPerHour { get; set; }
    public decimal ROIEstimate { get; set; } // Estimated value generated
    public List<CostSavingOpportunity> SavingOpportunities { get; set; } = new();
}

/// <summary>
/// Details of a costly team meeting
/// </summary>
public class CostlyTeamMeeting
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int AttendeeCount { get; set; }
    public double Duration { get; set; }
    public decimal Cost { get; set; }
    public List<string> Attendees { get; set; } = new();
}

/// <summary>
/// Opportunity to reduce meeting costs
/// </summary>
public class CostSavingOpportunity
{
    public string Description { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
    public string Category { get; set; } = string.Empty; // e.g., "Reduce attendees", "Shorten duration"
    public int Priority { get; set; } // 1-5
}

/// <summary>
/// Workload status for a team member
/// </summary>
public enum WorkloadStatus
{
    Underutilized,
    Optimal,
    Busy,
    Overloaded,
    Critical
}

/// <summary>
/// Comparison between two teams
/// </summary>
public class TeamComparison
{
    public TeamAnalyticsSummary Team1 { get; set; } = new();
    public TeamAnalyticsSummary Team2 { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }

    // Comparative metrics
    public List<MetricComparison> Comparisons { get; set; } = new();
    public string Winner { get; set; } = string.Empty; // Team with better overall metrics
    public List<string> Insights { get; set; } = new();
}

/// <summary>
/// Summary of team analytics for comparison
/// </summary>
public class TeamAnalyticsSummary
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public double HealthScore { get; set; }
    public int TotalMeetings { get; set; }
    public double TotalHours { get; set; }
    public decimal TotalCost { get; set; }
    public double ProductivityScore { get; set; }
}

/// <summary>
/// Comparison of a specific metric between teams
/// </summary>
public class MetricComparison
{
    public string MetricName { get; set; } = string.Empty;
    public double Team1Value { get; set; }
    public double Team2Value { get; set; }
    public double Difference { get; set; }
    public double DifferencePercentage { get; set; }
    public string BetterTeam { get; set; } = string.Empty;
}

/// <summary>
/// Organizational-wide analytics across multiple teams
/// </summary>
public class OrganizationalAnalytics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int TotalTeams { get; set; }
    public int TotalMembers { get; set; }

    // Aggregated organizational metrics
    public OrganizationalMetrics Metrics { get; set; } = new();

    // Per-team summaries
    public List<TeamAnalyticsSummary> TeamSummaries { get; set; } = new();

    // Cross-team analysis
    public CrossTeamAnalysis CrossTeamMetrics { get; set; } = new();

    // Organizational health
    public OrganizationalHealth Health { get; set; } = new();

    // Top performers and issues
    public List<TeamAnalyticsSummary> TopPerformingTeams { get; set; } = new();
    public List<TeamAnalyticsSummary> TeamsNeedingAttention { get; set; } = new();
}

/// <summary>
/// Organization-wide aggregated metrics
/// </summary>
public class OrganizationalMetrics
{
    public int TotalMeetings { get; set; }
    public double TotalMeetingHours { get; set; }
    public decimal TotalMeetingCost { get; set; }
    public double AverageMeetingDuration { get; set; }
    public decimal AverageCostPerMeeting { get; set; }

    // Per-member averages
    public double AverageMeetingsPerMember { get; set; }
    public double AverageHoursPerMember { get; set; }
    public decimal AverageCostPerMember { get; set; }

    // Productivity metrics
    public double AverageHealthScore { get; set; }
    public double AverageProductivityScore { get; set; }
    public int TotalFocusHours { get; set; }
}

/// <summary>
/// Cross-team collaboration and patterns
/// </summary>
public class CrossTeamAnalysis
{
    public int CrossTeamMeetings { get; set; }
    public double CrossTeamHours { get; set; }
    public decimal CrossTeamCost { get; set; }

    // Team collaboration pairs
    public List<TeamCollaborationPair> TopCollaboratingTeams { get; set; } = new();

    // Silos (teams with little cross-collaboration)
    public List<string> SiloedTeams { get; set; } = new();
}

/// <summary>
/// Collaboration between two teams
/// </summary>
public class TeamCollaborationPair
{
    public string Team1Name { get; set; } = string.Empty;
    public string Team2Name { get; set; } = string.Empty;
    public int MeetingCount { get; set; }
    public double TotalHours { get; set; }
    public decimal TotalCost { get; set; }
}

/// <summary>
/// Organizational health indicators
/// </summary>
public class OrganizationalHealth
{
    public double OverallScore { get; set; } // 0-100
    public double HealthScoreVariance { get; set; }
    public int HealthyTeams { get; set; }
    public int AtRiskTeams { get; set; }
    public int CriticalTeams { get; set; }

    public List<string> OrganizationalIssues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Trend analysis for a team over time
/// </summary>
public class TeamTrendAnalysis
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int HistoricalDays { get; set; }
    public int ForecastDays { get; set; }

    // Historical trends
    public List<TeamMetricTrend> Trends { get; set; } = new();

    // Predictions
    public List<TeamMetricPrediction> Predictions { get; set; } = new();

    // Insights
    public List<string> KeyInsights { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string TeamPattern { get; set; } = string.Empty; // e.g., "Growing", "Stable", "Declining"
}

/// <summary>
/// Trend for a specific team metric
/// </summary>
public class TeamMetricTrend
{
    public string MetricName { get; set; } = string.Empty;
    public List<DataPoint> DataPoints { get; set; } = new();
    public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
    public double TrendStrength { get; set; } // 0-100
    public double ChangePercentage { get; set; }
}

/// <summary>
/// Prediction for a team metric
/// </summary>
public class TeamMetricPrediction
{
    public string MetricName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double PredictedValue { get; set; }
    public double ConfidenceLevel { get; set; } // 0-100
    public DateTime PredictionDate { get; set; }
    public string Outlook { get; set; } = string.Empty; // Improving, Declining, Stable
}

/// <summary>
/// Data point for trend visualization
/// </summary>
public class DataPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
}

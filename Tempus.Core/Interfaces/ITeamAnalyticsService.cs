using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for generating team-level and organizational analytics
/// </summary>
public interface ITeamAnalyticsService
{
    /// <summary>
    /// Generate comprehensive analytics for a team
    /// </summary>
    Task<TeamAnalytics> GenerateTeamAnalyticsAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get quick stats for a team (last 30 days)
    /// </summary>
    Task<TeamMetrics> GetTeamQuickStatsAsync(Guid teamId, int days = 30);

    /// <summary>
    /// Calculate overall team health score
    /// </summary>
    Task<double> CalculateTeamHealthScoreAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get collaboration metrics for a team
    /// </summary>
    Task<TeamCollaborationMetrics> GetCollaborationMetricsAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Analyze workload balance across team members
    /// </summary>
    Task<WorkloadBalance> AnalyzeWorkloadBalanceAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get cost analysis for team meetings
    /// </summary>
    Task<TeamCostAnalysis> GetTeamCostAnalysisAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Compare analytics between two teams
    /// </summary>
    Task<TeamComparison> CompareTeamsAsync(Guid team1Id, Guid team2Id, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get analytics for a specific team member within team context
    /// </summary>
    Task<MemberAnalyticsSummary> GetMemberAnalyticsAsync(Guid teamId, string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get team recommendations based on analytics
    /// </summary>
    Task<List<string>> GetTeamRecommendationsAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Identify team members who may be overloaded or underutilized
    /// </summary>
    Task<Dictionary<WorkloadStatus, List<string>>> IdentifyWorkloadIssuesAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get organizational analytics across all teams
    /// </summary>
    Task<OrganizationalAnalytics> GenerateOrganizationalAnalyticsAsync(List<Guid> teamIds, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Generate trend analysis for a team over time
    /// </summary>
    Task<TeamTrendAnalysis> GenerateTeamTrendAnalysisAsync(Guid teamId, int historicalDays = 90, int forecastDays = 30);
}

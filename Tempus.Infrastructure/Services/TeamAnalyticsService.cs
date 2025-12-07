using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

/// <summary>
/// Service for generating team-level and organizational analytics
/// </summary>
public class TeamAnalyticsService : ITeamAnalyticsService
{
    private readonly ITeamService _teamService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<TeamAnalyticsService> _logger;

    public TeamAnalyticsService(
        ITeamService teamService,
        IAnalyticsService analyticsService,
        IEventRepository eventRepository,
        ILogger<TeamAnalyticsService> logger)
    {
        _teamService = teamService;
        _analyticsService = analyticsService;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<TeamAnalytics> GenerateTeamAnalyticsAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Generating team analytics for team {TeamId} from {StartDate} to {EndDate}",
            teamId, startDate, endDate);

        var team = await _teamService.GetTeamByIdAsync(teamId);
        if (team == null)
        {
            throw new ArgumentException($"Team with ID {teamId} not found");
        }

        var members = await _teamService.GetTeamMembersAsync(teamId);
        if (members.Count == 0)
        {
            _logger.LogWarning("Team {TeamId} has no members", teamId);
        }

        var teamAnalytics = new TeamAnalytics
        {
            TeamId = teamId,
            TeamName = team.Name,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            MemberCount = members.Count
        };

        // Generate analytics for each member
        var memberAnalyticsList = new List<MemberAnalyticsSummary>();
        var allMemberAnalytics = new List<CalendarAnalytics>();

        foreach (var member in members)
        {
            try
            {
                var analytics = await _analyticsService.GenerateAnalyticsAsync(member.UserId, startDate, endDate);
                allMemberAnalytics.Add(analytics);

                var memberSummary = await CreateMemberSummaryAsync(member, analytics);
                memberAnalyticsList.Add(memberSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics for team member {UserId}", member.UserId);
            }
        }

        teamAnalytics.MemberAnalytics = memberAnalyticsList;

        // Calculate aggregated metrics
        teamAnalytics.AggregatedMetrics = CalculateAggregatedMetrics(allMemberAnalytics, members.Count);

        // Calculate team vs individual comparisons
        UpdateMemberComparisons(teamAnalytics);

        // Generate collaboration metrics
        teamAnalytics.Collaboration = await GetCollaborationMetricsAsync(teamId, startDate, endDate);

        // Calculate health metrics
        teamAnalytics.Health = CalculateTeamHealthMetrics(teamAnalytics);

        // Generate cost analysis
        teamAnalytics.CostAnalysis = await GetTeamCostAnalysisAsync(teamId, startDate, endDate);

        _logger.LogInformation("Team analytics generated successfully for team {TeamId}", teamId);

        return teamAnalytics;
    }

    public async Task<TeamMetrics> GetTeamQuickStatsAsync(Guid teamId, int days = 30)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);

        var analytics = await GenerateTeamAnalyticsAsync(teamId, startDate, endDate);
        return analytics.AggregatedMetrics;
    }

    public async Task<double> CalculateTeamHealthScoreAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        var analytics = await GenerateTeamAnalyticsAsync(teamId, startDate, endDate);
        return analytics.Health.OverallHealthScore;
    }

    public async Task<TeamCollaborationMetrics> GetCollaborationMetricsAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        var members = await _teamService.GetTeamMembersAsync(teamId);
        var memberUserIds = members.Select(m => m.UserId).ToHashSet();

        var collaboration = new TeamCollaborationMetrics();

        // Get all events for team members in the period
        var allTeamEvents = new List<Event>();
        foreach (var member in members)
        {
            var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, member.UserId);
            allTeamEvents.AddRange(events.Where(e => e.EventType == EventType.Meeting));
        }

        // Analyze meetings
        var processedMeetings = new HashSet<Guid>();
        var collaborationPairs = new Dictionary<string, CollaborationPair>();

        foreach (var meeting in allTeamEvents)
        {
            if (processedMeetings.Contains(meeting.Id))
                continue;

            processedMeetings.Add(meeting.Id);

            // Get attendee emails
            var attendeeEmails = meeting.Attendees?.Select(a => a.Email).ToList() ?? new List<string>();

            var teamAttendeesInMeeting = attendeeEmails.Where(email =>
                members.Any(m => m.User?.Email == email)).ToList();
            var externalAttendeesCount = attendeeEmails.Count - teamAttendeesInMeeting.Count;

            var duration = (meeting.EndTime - meeting.StartTime).TotalHours;

            // Classify as internal or external
            if (teamAttendeesInMeeting.Count > 1 && externalAttendeesCount == 0)
            {
                collaboration.InternalMeetings++;
                collaboration.InternalMeetingHours += duration;
                collaboration.InternalMeetingCost += meeting.MeetingCost ?? 0;
            }
            else if (externalAttendeesCount > 0)
            {
                collaboration.ExternalMeetings++;
                collaboration.ExternalMeetingHours += duration;
                collaboration.ExternalMeetingCost += meeting.MeetingCost ?? 0;
            }

            // Track collaborations between team members
            if (teamAttendeesInMeeting.Count >= 2)
            {
                for (int i = 0; i < teamAttendeesInMeeting.Count - 1; i++)
                {
                    for (int j = i + 1; j < teamAttendeesInMeeting.Count; j++)
                    {
                        var member1 = teamAttendeesInMeeting[i];
                        var member2 = teamAttendeesInMeeting[j];
                        var key = string.CompareOrdinal(member1, member2) < 0
                            ? $"{member1}|{member2}"
                            : $"{member2}|{member1}";

                        if (!collaborationPairs.ContainsKey(key))
                        {
                            collaborationPairs[key] = new CollaborationPair
                            {
                                Member1 = member1,
                                Member2 = member2
                            };
                        }

                        collaborationPairs[key].MeetingCount++;
                        collaborationPairs[key].TotalHours += duration;
                    }
                }
            }
        }

        // Calculate averages and patterns
        var totalMeetings = collaboration.InternalMeetings + collaboration.ExternalMeetings;
        if (totalMeetings > 0)
        {
            var totalAttendees = allTeamEvents.Sum(e => e.Attendees?.Count ?? 0);
            collaboration.AverageMeetingSize = totalAttendees / (double)totalMeetings;
        }

        // Detect back-to-back meetings
        collaboration.BackToBackMeetings = CountBackToBackMeetings(allTeamEvents);

        // Get top collaborators
        collaboration.TopCollaborators = collaborationPairs.Values
            .OrderByDescending(c => c.MeetingCount)
            .Take(10)
            .ToList();

        // Identify isolated members (few collaborations)
        var memberCollaborationCounts = new Dictionary<string, int>();
        foreach (var member in members)
        {
            memberCollaborationCounts[member.UserId] = 0;
        }

        foreach (var pair in collaborationPairs.Values)
        {
            if (memberCollaborationCounts.ContainsKey(pair.Member1))
                memberCollaborationCounts[pair.Member1] += pair.MeetingCount;
            if (memberCollaborationCounts.ContainsKey(pair.Member2))
                memberCollaborationCounts[pair.Member2] += pair.MeetingCount;
        }

        var avgCollaboration = memberCollaborationCounts.Values.Any()
            ? memberCollaborationCounts.Values.Average()
            : 0;

        collaboration.IsolatedMembers = memberCollaborationCounts
            .Where(kvp => kvp.Value < avgCollaboration * 0.5) // Less than 50% of average
            .Select(kvp => kvp.Key)
            .ToList();

        return collaboration;
    }

    public async Task<WorkloadBalance> AnalyzeWorkloadBalanceAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        var analytics = await GenerateTeamAnalyticsAsync(teamId, startDate, endDate);
        return analytics.Health.Balance;
    }

    public async Task<TeamCostAnalysis> GetTeamCostAnalysisAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        var members = await _teamService.GetTeamMembersAsync(teamId);
        var memberUserIds = members.Select(m => m.UserId).ToHashSet();

        var costAnalysis = new TeamCostAnalysis();
        var allMeetings = new List<Event>();
        var costByType = new Dictionary<string, decimal>();

        // Collect all team meetings
        foreach (var member in members)
        {
            var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, member.UserId);
            var meetings = events.Where(e => e.EventType == EventType.Meeting && e.MeetingCost.HasValue);
            allMeetings.AddRange(meetings);
        }

        // Remove duplicates (same meeting attended by multiple team members)
        var uniqueMeetings = allMeetings
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .ToList();

        // Calculate costs
        foreach (var meeting in uniqueMeetings)
        {
            var cost = meeting.MeetingCost ?? 0;
            costAnalysis.TotalMeetingCost += cost;

            var typeName = meeting.EventType.ToString();
            if (!costByType.ContainsKey(typeName))
                costByType[typeName] = 0;
            costByType[typeName] += cost;
        }

        costAnalysis.CostByMeetingType = costByType;

        if (uniqueMeetings.Count > 0)
        {
            costAnalysis.AverageCostPerMeeting = costAnalysis.TotalMeetingCost / uniqueMeetings.Count;
        }

        if (members.Count > 0)
        {
            costAnalysis.CostPerMember = costAnalysis.TotalMeetingCost / members.Count;
        }

        // Calculate total meeting hours
        var totalHours = uniqueMeetings.Sum(m => (m.EndTime - m.StartTime).TotalHours);
        if (totalHours > 0)
        {
            costAnalysis.CostPerHour = costAnalysis.TotalMeetingCost / (decimal)totalHours;
        }

        // Get most costly meetings
        costAnalysis.MostCostlyMeetings = uniqueMeetings
            .OrderByDescending(m => m.MeetingCost ?? 0)
            .Take(10)
            .Select(m => new CostlyTeamMeeting
            {
                Title = m.Title,
                Date = m.StartTime,
                AttendeeCount = m.Attendees?.Count ?? 0,
                Duration = (m.EndTime - m.StartTime).TotalHours,
                Cost = m.MeetingCost ?? 0,
                Attendees = m.Attendees?.Select(a => a.Email).ToList() ?? new List<string>()
            })
            .ToList();

        // Identify cost saving opportunities
        costAnalysis.SavingOpportunities = IdentifyCostSavingOpportunities(uniqueMeetings);

        return costAnalysis;
    }

    public async Task<TeamComparison> CompareTeamsAsync(Guid team1Id, Guid team2Id, DateTime startDate, DateTime endDate)
    {
        var team1Analytics = await GenerateTeamAnalyticsAsync(team1Id, startDate, endDate);
        var team2Analytics = await GenerateTeamAnalyticsAsync(team2Id, startDate, endDate);

        var comparison = new TeamComparison
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            Team1 = new TeamAnalyticsSummary
            {
                TeamId = team1Analytics.TeamId,
                TeamName = team1Analytics.TeamName,
                MemberCount = team1Analytics.MemberCount,
                HealthScore = team1Analytics.Health.OverallHealthScore,
                TotalMeetings = team1Analytics.AggregatedMetrics.TotalMeetings,
                TotalHours = team1Analytics.AggregatedMetrics.TotalHours,
                TotalCost = team1Analytics.CostAnalysis.TotalMeetingCost,
                ProductivityScore = team1Analytics.Health.TeamProductivity
            },
            Team2 = new TeamAnalyticsSummary
            {
                TeamId = team2Analytics.TeamId,
                TeamName = team2Analytics.TeamName,
                MemberCount = team2Analytics.MemberCount,
                HealthScore = team2Analytics.Health.OverallHealthScore,
                TotalMeetings = team2Analytics.AggregatedMetrics.TotalMeetings,
                TotalHours = team2Analytics.AggregatedMetrics.TotalHours,
                TotalCost = team2Analytics.CostAnalysis.TotalMeetingCost,
                ProductivityScore = team2Analytics.Health.TeamProductivity
            }
        };

        // Generate metric comparisons
        comparison.Comparisons = GenerateMetricComparisons(comparison.Team1, comparison.Team2);

        // Determine winner (team with better overall metrics)
        var team1Score = comparison.Comparisons.Count(c => c.BetterTeam == comparison.Team1.TeamName);
        var team2Score = comparison.Comparisons.Count(c => c.BetterTeam == comparison.Team2.TeamName);

        comparison.Winner = team1Score > team2Score ? comparison.Team1.TeamName :
                           team2Score > team1Score ? comparison.Team2.TeamName : "Tie";

        // Generate insights
        comparison.Insights = GenerateComparisonInsights(comparison);

        return comparison;
    }

    public async Task<MemberAnalyticsSummary> GetMemberAnalyticsAsync(Guid teamId, string userId, DateTime startDate, DateTime endDate)
    {
        var teamMember = await _teamService.GetTeamMemberAsync(teamId, userId);
        if (teamMember == null)
        {
            throw new ArgumentException($"User {userId} is not a member of team {teamId}");
        }

        var analytics = await _analyticsService.GenerateAnalyticsAsync(userId, startDate, endDate);
        var summary = await CreateMemberSummaryAsync(teamMember, analytics);

        // Get team averages for comparison
        var teamAnalytics = await GenerateTeamAnalyticsAsync(teamId, startDate, endDate);
        var teamMetrics = teamAnalytics.AggregatedMetrics;

        summary.EventsVsTeamAverage = CalculatePercentageDifference(summary.EventCount, teamMetrics.AverageEventsPerMember);
        summary.MeetingsVsTeamAverage = CalculatePercentageDifference(summary.MeetingCount, teamMetrics.AverageMeetingsPerMember);
        summary.HoursVsTeamAverage = CalculatePercentageDifference(summary.TotalHours, teamMetrics.AverageHoursPerMember);
        summary.HealthScoreVsTeamAverage = CalculatePercentageDifference(summary.HealthScore, teamAnalytics.Health.AverageHealthScore);

        return summary;
    }

    public async Task<List<string>> GetTeamRecommendationsAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        var analytics = await GenerateTeamAnalyticsAsync(teamId, startDate, endDate);
        var recommendations = new List<string>();

        // Health-based recommendations
        if (analytics.Health.OverallHealthScore < 60)
        {
            recommendations.Add("⚠️ Team health score is below optimal. Consider reducing meeting load and increasing focus time.");
        }

        // Workload balance recommendations
        if (analytics.Health.OverloadedMembers > 0)
        {
            recommendations.Add($"📊 {analytics.Health.OverloadedMembers} team member(s) are overloaded. Consider redistributing workload.");
        }

        if (analytics.Health.UnderutilizedMembers > 0)
        {
            recommendations.Add($"💡 {analytics.Health.UnderutilizedMembers} team member(s) may have capacity for additional work.");
        }

        // Collaboration recommendations
        if (analytics.Collaboration.IsolatedMembers.Count > 0)
        {
            recommendations.Add($"🤝 {analytics.Collaboration.IsolatedMembers.Count} team member(s) have limited collaboration. Consider cross-functional pairing.");
        }

        // Cost recommendations
        if (analytics.CostAnalysis.TotalMeetingCost > 0)
        {
            var avgCost = analytics.CostAnalysis.AverageCostPerMeeting;
            if (avgCost > 500)
            {
                recommendations.Add("💰 Average meeting cost is high. Consider reducing attendee count or meeting duration.");
            }
        }

        // Meeting pattern recommendations
        if (analytics.Collaboration.BackToBackMeetings > analytics.AggregatedMetrics.TotalMeetings * 0.3)
        {
            recommendations.Add("⏰ High number of back-to-back meetings. Schedule buffer time between meetings.");
        }

        // Focus time recommendations
        var avgFocusHours = analytics.AggregatedMetrics.AverageFocusHoursPerMember;
        if (avgFocusHours < 10)
        {
            recommendations.Add("🎯 Team has limited focus time. Block out dedicated focus periods on calendars.");
        }

        return recommendations;
    }

    public async Task<Dictionary<WorkloadStatus, List<string>>> IdentifyWorkloadIssuesAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        var analytics = await GenerateTeamAnalyticsAsync(teamId, startDate, endDate);

        var issues = new Dictionary<WorkloadStatus, List<string>>
        {
            [WorkloadStatus.Critical] = new List<string>(),
            [WorkloadStatus.Overloaded] = new List<string>(),
            [WorkloadStatus.Optimal] = new List<string>(),
            [WorkloadStatus.Busy] = new List<string>(),
            [WorkloadStatus.Underutilized] = new List<string>()
        };

        foreach (var member in analytics.MemberAnalytics)
        {
            issues[member.WorkloadStatus].Add(member.UserName);
        }

        return issues;
    }

    public async Task<OrganizationalAnalytics> GenerateOrganizationalAnalyticsAsync(List<Guid> teamIds, DateTime startDate, DateTime endDate)
    {
        var orgAnalytics = new OrganizationalAnalytics
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            TotalTeams = teamIds.Count
        };

        var allTeamAnalytics = new List<TeamAnalytics>();

        // Generate analytics for each team
        foreach (var teamId in teamIds)
        {
            try
            {
                var teamAnalytics = await GenerateTeamAnalyticsAsync(teamId, startDate, endDate);
                allTeamAnalytics.Add(teamAnalytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics for team {TeamId}", teamId);
            }
        }

        orgAnalytics.TotalMembers = allTeamAnalytics.Sum(t => t.MemberCount);

        // Create team summaries
        orgAnalytics.TeamSummaries = allTeamAnalytics.Select(t => new TeamAnalyticsSummary
        {
            TeamId = t.TeamId,
            TeamName = t.TeamName,
            MemberCount = t.MemberCount,
            HealthScore = t.Health.OverallHealthScore,
            TotalMeetings = t.AggregatedMetrics.TotalMeetings,
            TotalHours = t.AggregatedMetrics.TotalHours,
            TotalCost = t.CostAnalysis.TotalMeetingCost,
            ProductivityScore = t.Health.TeamProductivity
        }).ToList();

        // Calculate organizational metrics
        orgAnalytics.Metrics = new OrganizationalMetrics
        {
            TotalMeetings = allTeamAnalytics.Sum(t => t.AggregatedMetrics.TotalMeetings),
            TotalMeetingHours = allTeamAnalytics.Sum(t => t.AggregatedMetrics.TotalHours),
            TotalMeetingCost = allTeamAnalytics.Sum(t => t.CostAnalysis.TotalMeetingCost),
            AverageHealthScore = allTeamAnalytics.Average(t => t.Health.OverallHealthScore),
            AverageProductivityScore = allTeamAnalytics.Average(t => t.Health.TeamProductivity),
            TotalFocusHours = allTeamAnalytics.Sum(t => t.AggregatedMetrics.TotalFocusHours)
        };

        if (orgAnalytics.Metrics.TotalMeetings > 0)
        {
            orgAnalytics.Metrics.AverageMeetingDuration =
                orgAnalytics.Metrics.TotalMeetingHours / orgAnalytics.Metrics.TotalMeetings;
            orgAnalytics.Metrics.AverageCostPerMeeting =
                orgAnalytics.Metrics.TotalMeetingCost / orgAnalytics.Metrics.TotalMeetings;
        }

        if (orgAnalytics.TotalMembers > 0)
        {
            orgAnalytics.Metrics.AverageMeetingsPerMember =
                orgAnalytics.Metrics.TotalMeetings / (double)orgAnalytics.TotalMembers;
            orgAnalytics.Metrics.AverageHoursPerMember =
                orgAnalytics.Metrics.TotalMeetingHours / orgAnalytics.TotalMembers;
            orgAnalytics.Metrics.AverageCostPerMember =
                orgAnalytics.Metrics.TotalMeetingCost / orgAnalytics.TotalMembers;
        }

        // Identify top and bottom performers
        orgAnalytics.TopPerformingTeams = orgAnalytics.TeamSummaries
            .OrderByDescending(t => t.HealthScore)
            .Take(5)
            .ToList();

        orgAnalytics.TeamsNeedingAttention = orgAnalytics.TeamSummaries
            .OrderBy(t => t.HealthScore)
            .Take(5)
            .ToList();

        // Calculate organizational health
        orgAnalytics.Health = CalculateOrganizationalHealth(allTeamAnalytics);

        return orgAnalytics;
    }

    public async Task<TeamTrendAnalysis> GenerateTeamTrendAnalysisAsync(Guid teamId, int historicalDays = 90, int forecastDays = 30)
    {
        var team = await _teamService.GetTeamByIdAsync(teamId);
        if (team == null)
        {
            throw new ArgumentException($"Team with ID {teamId} not found");
        }

        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-historicalDays);

        var trendAnalysis = new TeamTrendAnalysis
        {
            TeamId = teamId,
            TeamName = team.Name,
            AnalysisDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            HistoricalDays = historicalDays,
            ForecastDays = forecastDays
        };

        // Collect historical data points (weekly)
        var weeklyDataPoints = new List<(DateTime Date, TeamMetrics Metrics)>();
        var currentDate = startDate;

        while (currentDate < endDate)
        {
            var weekEnd = currentDate.AddDays(7);
            if (weekEnd > endDate) weekEnd = endDate;

            try
            {
                var metrics = await GetTeamQuickStatsAsync(teamId, (weekEnd - currentDate).Days);
                weeklyDataPoints.Add((currentDate, metrics));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get metrics for week starting {Date}", currentDate);
            }

            currentDate = currentDate.AddDays(7);
        }

        // Generate trends
        trendAnalysis.Trends = GenerateTeamTrends(weeklyDataPoints);

        // Generate predictions (simple linear projection)
        trendAnalysis.Predictions = GenerateTeamPredictions(weeklyDataPoints, forecastDays);

        // Determine team pattern
        if (trendAnalysis.Trends.Any(t => t.TrendDirection == "Up" && t.MetricName.Contains("Health")))
        {
            trendAnalysis.TeamPattern = "Improving";
        }
        else if (trendAnalysis.Trends.Any(t => t.TrendDirection == "Down" && t.MetricName.Contains("Health")))
        {
            trendAnalysis.TeamPattern = "Declining";
        }
        else
        {
            trendAnalysis.TeamPattern = "Stable";
        }

        // Generate insights
        trendAnalysis.KeyInsights = GenerateTrendInsights(trendAnalysis);

        return trendAnalysis;
    }

    #region Helper Methods

    private async Task<MemberAnalyticsSummary> CreateMemberSummaryAsync(TeamMember member, CalendarAnalytics analytics)
    {
        var summary = new MemberAnalyticsSummary
        {
            UserId = member.UserId,
            UserName = member.User?.UserName ?? "Unknown",
            Email = member.User?.Email ?? "",
            Role = member.Role,
            EventCount = analytics.TotalEvents,
            MeetingCount = analytics.MeetingStats.TotalMeetings,
            TotalHours = analytics.TotalHours,
            MeetingCost = analytics.TotalMeetingCost,
            HealthScore = await _analyticsService.CalculateCalendarHealthScoreAsync(member.UserId, analytics.StartDate, analytics.EndDate),
            FocusTimeHours = analytics.Productivity.TotalFocusHours
        };

        // Determine workload status
        summary.WorkloadStatus = DetermineWorkloadStatus(summary.TotalHours, summary.HealthScore);

        // Generate member-specific insights
        summary.Insights = GenerateMemberInsights(summary, analytics);

        return summary;
    }

    private TeamMetrics CalculateAggregatedMetrics(List<CalendarAnalytics> allMemberAnalytics, int memberCount)
    {
        var metrics = new TeamMetrics();

        if (allMemberAnalytics.Count == 0)
            return metrics;

        metrics.TotalEvents = allMemberAnalytics.Sum(a => a.TotalEvents);
        metrics.TotalMeetings = allMemberAnalytics.Sum(a => a.MeetingStats.TotalMeetings);
        metrics.TotalHours = allMemberAnalytics.Sum(a => a.TotalHours);
        metrics.TotalMeetingCost = allMemberAnalytics.Sum(a => a.TotalMeetingCost);
        metrics.TotalFocusHours = allMemberAnalytics.Sum(a => a.Productivity.TotalFocusHours);

        if (memberCount > 0)
        {
            metrics.AverageEventsPerMember = metrics.TotalEvents / (double)memberCount;
            metrics.AverageMeetingsPerMember = metrics.TotalMeetings / (double)memberCount;
            metrics.AverageHoursPerMember = metrics.TotalHours / memberCount;
            metrics.AverageCostPerMember = metrics.TotalMeetingCost / memberCount;
            metrics.AverageFocusHoursPerMember = metrics.TotalFocusHours / memberCount;
        }

        // Aggregate event type distributions
        metrics.EventTypeDistribution = new Dictionary<string, int>();
        foreach (var analytics in allMemberAnalytics)
        {
            foreach (var kvp in analytics.TimeUsage.EventTypeCounts)
            {
                if (!metrics.EventTypeDistribution.ContainsKey(kvp.Key))
                    metrics.EventTypeDistribution[kvp.Key] = 0;
                metrics.EventTypeDistribution[kvp.Key] += kvp.Value;
            }
        }

        // Aggregate day-of-week distribution
        metrics.BusiestDays = new Dictionary<DayOfWeek, int>();
        foreach (var analytics in allMemberAnalytics)
        {
            foreach (var kvp in analytics.TimeUsage.DayOfWeekHours)
            {
                if (!metrics.BusiestDays.ContainsKey(kvp.Key))
                    metrics.BusiestDays[kvp.Key] = 0;
                metrics.BusiestDays[kvp.Key] += kvp.Value;
            }
        }

        // Aggregate hour-of-day distribution
        metrics.BusiestHours = new Dictionary<int, int>();
        foreach (var analytics in allMemberAnalytics)
        {
            foreach (var kvp in analytics.TimeUsage.HourOfDayDistribution)
            {
                if (!metrics.BusiestHours.ContainsKey(kvp.Key))
                    metrics.BusiestHours[kvp.Key] = 0;
                metrics.BusiestHours[kvp.Key] += kvp.Value;
            }
        }

        return metrics;
    }

    private void UpdateMemberComparisons(TeamAnalytics teamAnalytics)
    {
        var teamMetrics = teamAnalytics.AggregatedMetrics;
        var healthScores = teamAnalytics.MemberAnalytics.Select(m => m.HealthScore).ToList();
        var avgHealthScore = healthScores.Any() ? healthScores.Average() : 0;

        foreach (var member in teamAnalytics.MemberAnalytics)
        {
            member.EventsVsTeamAverage = CalculatePercentageDifference(member.EventCount, teamMetrics.AverageEventsPerMember);
            member.MeetingsVsTeamAverage = CalculatePercentageDifference(member.MeetingCount, teamMetrics.AverageMeetingsPerMember);
            member.HoursVsTeamAverage = CalculatePercentageDifference(member.TotalHours, teamMetrics.AverageHoursPerMember);
            member.HealthScoreVsTeamAverage = CalculatePercentageDifference(member.HealthScore, avgHealthScore);
        }
    }

    private TeamHealthMetrics CalculateTeamHealthMetrics(TeamAnalytics teamAnalytics)
    {
        var health = new TeamHealthMetrics();
        var memberAnalytics = teamAnalytics.MemberAnalytics;

        if (memberAnalytics.Count == 0)
            return health;

        // Calculate average health score
        health.AverageHealthScore = memberAnalytics.Average(m => m.HealthScore);

        // Calculate health score variance
        var healthScores = memberAnalytics.Select(m => m.HealthScore).ToList();
        health.HealthScoreVariance = CalculateVariance(healthScores);

        // Calculate overall team health (weighted average)
        health.OverallHealthScore = health.AverageHealthScore;

        // Count workload statuses
        health.OverloadedMembers = memberAnalytics.Count(m =>
            m.WorkloadStatus == WorkloadStatus.Overloaded || m.WorkloadStatus == WorkloadStatus.Critical);
        health.UnderutilizedMembers = memberAnalytics.Count(m => m.WorkloadStatus == WorkloadStatus.Underutilized);
        health.OptimalMembers = memberAnalytics.Count(m => m.WorkloadStatus == WorkloadStatus.Optimal);

        // Calculate workload balance
        health.Balance = CalculateWorkloadBalance(memberAnalytics);

        // Calculate team productivity
        health.TeamProductivity = CalculateTeamProductivity(teamAnalytics);
        health.TotalFocusBlocks = memberAnalytics.Sum(m => m.FocusTimeHours / 2); // Assume 2-hour blocks

        // Calculate focus time balance
        var focusTimes = memberAnalytics.Select(m => (double)m.FocusTimeHours).ToList();
        health.FocusTimeBalance = CalculateVariance(focusTimes);

        // Generate health issues and recommendations
        health.HealthIssues = IdentifyHealthIssues(teamAnalytics);
        health.Recommendations = GenerateHealthRecommendations(health);

        return health;
    }

    private WorkloadBalance CalculateWorkloadBalance(List<MemberAnalyticsSummary> memberAnalytics)
    {
        var balance = new WorkloadBalance();

        if (memberAnalytics.Count == 0)
        {
            balance.Status = "No Data";
            return balance;
        }

        var hours = memberAnalytics.Select(m => m.TotalHours).ToList();
        var average = hours.Average();
        balance.StandardDeviation = CalculateStandardDeviation(hours);

        // Calculate balance score (0-100, higher is better)
        // Lower standard deviation = better balance
        var coefficientOfVariation = average > 0 ? balance.StandardDeviation / average : 0;
        balance.BalanceScore = Math.Max(0, 100 - (coefficientOfVariation * 100));

        // Determine status
        if (balance.BalanceScore >= 80)
        {
            balance.Status = "Balanced";
        }
        else if (balance.BalanceScore >= 60)
        {
            balance.Status = "Moderate Imbalance";
        }
        else
        {
            balance.Status = "Severe Imbalance";
        }

        // Identify specific imbalances
        var overloaded = memberAnalytics.Where(m => m.TotalHours > average * 1.3).ToList();
        var underutilized = memberAnalytics.Where(m => m.TotalHours < average * 0.7).ToList();

        if (overloaded.Any())
        {
            balance.Imbalances.Add($"{overloaded.Count} member(s) working significantly above average");
        }

        if (underutilized.Any())
        {
            balance.Imbalances.Add($"{underutilized.Count} member(s) working significantly below average");
        }

        return balance;
    }

    private double CalculateTeamProductivity(TeamAnalytics teamAnalytics)
    {
        // Productivity based on health score and focus time
        var healthScore = teamAnalytics.Health.AverageHealthScore;
        var avgFocusTime = teamAnalytics.AggregatedMetrics.AverageFocusHoursPerMember;

        // Target is 20+ hours of focus time per period
        var focusTimeScore = Math.Min(100, (avgFocusTime / 20.0) * 100);

        // Weighted average: 60% health, 40% focus time
        return (healthScore * 0.6) + (focusTimeScore * 0.4);
    }

    private WorkloadStatus DetermineWorkloadStatus(double totalHours, double healthScore)
    {
        // Criteria based on hours and health score
        if (totalHours > 60 || healthScore < 40)
            return WorkloadStatus.Critical;

        if (totalHours > 50 || healthScore < 60)
            return WorkloadStatus.Overloaded;

        if (totalHours < 20)
            return WorkloadStatus.Underutilized;

        if (totalHours >= 35 && totalHours <= 45 && healthScore >= 70)
            return WorkloadStatus.Optimal;

        return WorkloadStatus.Busy;
    }

    private List<string> GenerateMemberInsights(MemberAnalyticsSummary summary, CalendarAnalytics analytics)
    {
        var insights = new List<string>();

        if (summary.HealthScore < 60)
        {
            insights.Add("Health score needs improvement");
        }

        if (summary.MeetingCount > 40)
        {
            insights.Add("High meeting load");
        }

        if (summary.FocusTimeHours < 10)
        {
            insights.Add("Limited focus time available");
        }

        if (analytics.MeetingStats.BackToBackMeetings > 5)
        {
            insights.Add("Frequent back-to-back meetings");
        }

        return insights;
    }

    private List<string> IdentifyHealthIssues(TeamAnalytics teamAnalytics)
    {
        var issues = new List<string>();

        if (teamAnalytics.Health.OverallHealthScore < 60)
        {
            issues.Add("Overall team health score is below optimal threshold");
        }

        if (teamAnalytics.Health.OverloadedMembers > teamAnalytics.MemberCount * 0.3)
        {
            issues.Add("More than 30% of team members are overloaded");
        }

        if (teamAnalytics.Health.Balance.BalanceScore < 60)
        {
            issues.Add("Significant workload imbalance across team");
        }

        if (teamAnalytics.Collaboration.IsolatedMembers.Count > 0)
        {
            issues.Add($"{teamAnalytics.Collaboration.IsolatedMembers.Count} team member(s) have limited collaboration");
        }

        return issues;
    }

    private List<string> GenerateHealthRecommendations(TeamHealthMetrics health)
    {
        var recommendations = new List<string>();

        if (health.OverloadedMembers > 0)
        {
            recommendations.Add("Redistribute workload to balance team capacity");
        }

        if (health.FocusTimeBalance > 10)
        {
            recommendations.Add("Ensure all team members have adequate focus time");
        }

        if (health.OverallHealthScore < 70)
        {
            recommendations.Add("Implement regular check-ins to address team wellness");
        }

        return recommendations;
    }

    private List<CostSavingOpportunity> IdentifyCostSavingOpportunities(List<Event> meetings)
    {
        var opportunities = new List<CostSavingOpportunity>();

        // Identify long meetings that could be shortened
        var longMeetings = meetings.Where(m => (m.EndTime - m.StartTime).TotalMinutes > 60).ToList();
        if (longMeetings.Any())
        {
            var potentialSavings = longMeetings.Sum(m => (m.MeetingCost ?? 0) * 0.25m); // 25% savings
            opportunities.Add(new CostSavingOpportunity
            {
                Description = $"Shorten {longMeetings.Count} meetings over 60 minutes",
                PotentialSavings = potentialSavings,
                Category = "Shorten duration",
                Priority = 3
            });
        }

        // Identify meetings with many attendees
        var largeMeetings = meetings.Where(m => (m.Attendees?.Count ?? 0) > 8).ToList();
        if (largeMeetings.Any())
        {
            var potentialSavings = largeMeetings.Sum(m => (m.MeetingCost ?? 0) * 0.30m); // 30% savings
            opportunities.Add(new CostSavingOpportunity
            {
                Description = $"Reduce attendees in {largeMeetings.Count} meetings with 8+ participants",
                PotentialSavings = potentialSavings,
                Category = "Reduce attendees",
                Priority = 4
            });
        }

        return opportunities.OrderByDescending(o => o.Priority).ToList();
    }

    private int CountBackToBackMeetings(List<Event> events)
    {
        var sortedEvents = events.OrderBy(e => e.StartTime).ToList();
        var backToBackCount = 0;

        for (int i = 0; i < sortedEvents.Count - 1; i++)
        {
            var timeBetween = (sortedEvents[i + 1].StartTime - sortedEvents[i].EndTime).TotalMinutes;
            if (timeBetween < 5) // Less than 5 minutes between meetings
            {
                backToBackCount++;
            }
        }

        return backToBackCount;
    }

    private List<MetricComparison> GenerateMetricComparisons(TeamAnalyticsSummary team1, TeamAnalyticsSummary team2)
    {
        var comparisons = new List<MetricComparison>();

        comparisons.Add(CreateMetricComparison("Health Score", team1.HealthScore, team2.HealthScore, team1.TeamName, team2.TeamName, true));
        comparisons.Add(CreateMetricComparison("Total Meetings", team1.TotalMeetings, team2.TotalMeetings, team1.TeamName, team2.TeamName, false));
        comparisons.Add(CreateMetricComparison("Total Hours", team1.TotalHours, team2.TotalHours, team1.TeamName, team2.TeamName, false));
        comparisons.Add(CreateMetricComparison("Productivity Score", team1.ProductivityScore, team2.ProductivityScore, team1.TeamName, team2.TeamName, true));
        comparisons.Add(CreateMetricComparison("Meeting Cost", (double)team1.TotalCost, (double)team2.TotalCost, team1.TeamName, team2.TeamName, false));

        return comparisons;
    }

    private MetricComparison CreateMetricComparison(string name, double value1, double value2,
        string team1Name, string team2Name, bool higherIsBetter)
    {
        var comparison = new MetricComparison
        {
            MetricName = name,
            Team1Value = value1,
            Team2Value = value2,
            Difference = value1 - value2,
            DifferencePercentage = value2 != 0 ? ((value1 - value2) / value2) * 100 : 0
        };

        if (Math.Abs(comparison.Difference) < 0.01)
        {
            comparison.BetterTeam = "Tie";
        }
        else if (higherIsBetter)
        {
            comparison.BetterTeam = value1 > value2 ? team1Name : team2Name;
        }
        else
        {
            comparison.BetterTeam = value1 < value2 ? team1Name : team2Name;
        }

        return comparison;
    }

    private List<string> GenerateComparisonInsights(TeamComparison comparison)
    {
        var insights = new List<string>();

        var healthDiff = comparison.Team1.HealthScore - comparison.Team2.HealthScore;
        if (Math.Abs(healthDiff) > 10)
        {
            var better = healthDiff > 0 ? comparison.Team1.TeamName : comparison.Team2.TeamName;
            insights.Add($"{better} has significantly better team health");
        }

        var sizeDiff = comparison.Team1.MemberCount - comparison.Team2.MemberCount;
        if (Math.Abs(sizeDiff) > 3)
        {
            insights.Add($"Team sizes differ significantly ({Math.Abs(sizeDiff)} members)");
        }

        return insights;
    }

    private OrganizationalHealth CalculateOrganizationalHealth(List<TeamAnalytics> allTeamAnalytics)
    {
        var health = new OrganizationalHealth();

        if (allTeamAnalytics.Count == 0)
            return health;

        var healthScores = allTeamAnalytics.Select(t => t.Health.OverallHealthScore).ToList();
        health.OverallScore = healthScores.Average();
        health.HealthScoreVariance = CalculateVariance(healthScores);

        // Categorize teams
        foreach (var team in allTeamAnalytics)
        {
            if (team.Health.OverallHealthScore >= 70)
                health.HealthyTeams++;
            else if (team.Health.OverallHealthScore >= 50)
                health.AtRiskTeams++;
            else
                health.CriticalTeams++;
        }

        // Generate organizational issues
        if (health.CriticalTeams > 0)
        {
            health.OrganizationalIssues.Add($"{health.CriticalTeams} team(s) in critical health status");
        }

        if (health.HealthScoreVariance > 200)
        {
            health.OrganizationalIssues.Add("High variance in team health scores indicates inconsistent team management");
        }

        // Generate recommendations
        if (health.OverallScore < 65)
        {
            health.Recommendations.Add("Implement organization-wide wellness initiatives");
        }

        if (health.AtRiskTeams + health.CriticalTeams > health.HealthyTeams)
        {
            health.Recommendations.Add("Majority of teams need attention - consider leadership intervention");
        }

        return health;
    }

    private List<TeamMetricTrend> GenerateTeamTrends(List<(DateTime Date, TeamMetrics Metrics)> dataPoints)
    {
        var trends = new List<TeamMetricTrend>();

        if (dataPoints.Count < 2)
            return trends;

        // Health score trend
        var healthTrend = new TeamMetricTrend { MetricName = "Team Health Score" };
        // Note: We'd need to store health scores in data points for this to work properly
        trends.Add(healthTrend);

        // Meeting count trend
        var meetingTrend = new TeamMetricTrend { MetricName = "Total Meetings" };
        meetingTrend.DataPoints = dataPoints.Select(dp => new DataPoint
        {
            Date = dp.Date,
            Value = dp.Metrics.TotalMeetings
        }).ToList();

        var firstMeetings = dataPoints.First().Metrics.TotalMeetings;
        var lastMeetings = dataPoints.Last().Metrics.TotalMeetings;
        meetingTrend.ChangePercentage = firstMeetings > 0
            ? ((lastMeetings - firstMeetings) / (double)firstMeetings) * 100
            : 0;
        meetingTrend.TrendDirection = meetingTrend.ChangePercentage > 5 ? "Up" :
                                       meetingTrend.ChangePercentage < -5 ? "Down" : "Stable";
        trends.Add(meetingTrend);

        return trends;
    }

    private List<TeamMetricPrediction> GenerateTeamPredictions(
        List<(DateTime Date, TeamMetrics Metrics)> dataPoints, int forecastDays)
    {
        var predictions = new List<TeamMetricPrediction>();

        if (dataPoints.Count < 2)
            return predictions;

        // Simple linear projection for meetings
        var meetings = dataPoints.Select(dp => (double)dp.Metrics.TotalMeetings).ToList();
        var avgMeetings = meetings.Average();
        var trend = meetings.Last() - meetings.First();

        predictions.Add(new TeamMetricPrediction
        {
            MetricName = "Total Meetings",
            CurrentValue = meetings.Last(),
            PredictedValue = meetings.Last() + trend,
            ConfidenceLevel = 60,
            PredictionDate = DateTime.Today.AddDays(forecastDays),
            Outlook = trend > 0 ? "Increasing" : trend < 0 ? "Declining" : "Stable"
        });

        return predictions;
    }

    private List<string> GenerateTrendInsights(TeamTrendAnalysis analysis)
    {
        var insights = new List<string>();

        foreach (var trend in analysis.Trends)
        {
            if (Math.Abs(trend.ChangePercentage) > 20)
            {
                insights.Add($"{trend.MetricName} has changed by {trend.ChangePercentage:F1}% ({trend.TrendDirection})");
            }
        }

        if (analysis.TeamPattern == "Declining")
        {
            insights.Add("⚠️ Team metrics show declining trend - intervention may be needed");
        }
        else if (analysis.TeamPattern == "Improving")
        {
            insights.Add("✅ Team metrics show positive improvement");
        }

        return insights;
    }

    private double CalculatePercentageDifference(double value, double average)
    {
        if (average == 0) return 0;
        return ((value - average) / average) * 100;
    }

    private double CalculatePercentageDifference(int value, double average)
    {
        return CalculatePercentageDifference((double)value, average);
    }

    private double CalculateVariance(List<double> values)
    {
        if (values.Count == 0) return 0;

        var mean = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return sumOfSquares / values.Count;
    }

    private double CalculateStandardDeviation(List<double> values)
    {
        return Math.Sqrt(CalculateVariance(values));
    }

    #endregion
}

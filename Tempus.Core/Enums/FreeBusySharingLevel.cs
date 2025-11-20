namespace Tempus.Core.Enums;

/// <summary>
/// Defines who can view a user's free/busy information
/// </summary>
public enum FreeBusySharingLevel
{
    /// <summary>
    /// Don't share free/busy information with anyone
    /// </summary>
    None = 0,

    /// <summary>
    /// Share free/busy information only with team members
    /// </summary>
    TeamMembers = 1,

    /// <summary>
    /// Share free/busy information with all authenticated users in the organization
    /// </summary>
    Organization = 2,

    /// <summary>
    /// Share free/busy information publicly (anyone with link)
    /// </summary>
    Public = 3
}

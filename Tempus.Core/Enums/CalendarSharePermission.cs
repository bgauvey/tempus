namespace Tempus.Core.Enums;

/// <summary>
/// Permission levels for calendar sharing
/// </summary>
public enum CalendarSharePermission
{
    /// <summary>
    /// Can only see free/busy times (no event details)
    /// </summary>
    FreeBusyOnly = 0,

    /// <summary>
    /// Can see all event details but cannot modify
    /// </summary>
    ViewAll = 1,

    /// <summary>
    /// Can make changes to events (create, edit, delete)
    /// </summary>
    Edit = 2,

    /// <summary>
    /// Can make changes to events and manage sharing permissions
    /// </summary>
    ManageSharing = 3
}

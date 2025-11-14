namespace Tempus.Core.Enums;

/// <summary>
/// Types of availability status a user can set
/// </summary>
public enum AvailabilityStatusType
{
    /// <summary>
    /// Standard out of office (vacation, sick leave, etc.)
    /// </summary>
    OutOfOffice = 0,

    /// <summary>
    /// Focus time - block time for deep work, auto-decline meetings
    /// </summary>
    FocusTime = 1,

    /// <summary>
    /// Working remotely but available
    /// </summary>
    WorkingRemotely = 2,

    /// <summary>
    /// In a meeting or busy
    /// </summary>
    Busy = 3,

    /// <summary>
    /// Do not disturb - more restrictive than focus time
    /// </summary>
    DoNotDisturb = 4
}

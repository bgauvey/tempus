namespace Tempus.Core.Enums;

/// <summary>
/// Represents different working location types
/// </summary>
public enum WorkingLocationType
{
    /// <summary>
    /// Working from the office/workplace
    /// </summary>
    Office = 0,

    /// <summary>
    /// Working from home
    /// </summary>
    Home = 1,

    /// <summary>
    /// Working remotely from a location other than home or office
    /// </summary>
    Remote = 2,

    /// <summary>
    /// Traveling (business trip, client site, etc.)
    /// </summary>
    Traveling = 3,

    /// <summary>
    /// Hybrid - split time between office and home
    /// </summary>
    Hybrid = 4,

    /// <summary>
    /// Not specified
    /// </summary>
    NotSet = 5
}

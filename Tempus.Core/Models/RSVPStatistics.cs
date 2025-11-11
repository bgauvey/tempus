namespace Tempus.Core.Models;

/// <summary>
/// Statistics about RSVP responses for an event
/// </summary>
public class RSVPStatistics
{
    public int TotalInvited { get; set; }
    public int Accepted { get; set; }
    public int Declined { get; set; }
    public int Tentative { get; set; }
    public int Pending { get; set; }
    public double ResponseRate => TotalInvited > 0 ? (double)(Accepted + Declined + Tentative) / TotalInvited * 100 : 0;
}

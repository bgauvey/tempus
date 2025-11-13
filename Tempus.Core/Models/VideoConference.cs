using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class VideoConference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public VideoConferenceProvider Provider { get; set; }

    // Meeting details
    public string MeetingUrl { get; set; } = string.Empty;
    public string? MeetingId { get; set; }
    public string? Passcode { get; set; }

    // Phone dial-in
    public string? DialInNumbers { get; set; } // JSON array of dial-in numbers
    public string? DialInPasscode { get; set; }

    // Provider-specific
    public string? HostKey { get; set; } // Zoom host key
    public string? ExternalMeetingId { get; set; } // Provider's internal meeting ID

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty; // User ID

    // Helper methods
    public string GetProviderDisplayName()
    {
        return Provider switch
        {
            VideoConferenceProvider.Zoom => "Zoom",
            VideoConferenceProvider.MicrosoftTeams => "Microsoft Teams",
            VideoConferenceProvider.GoogleMeet => "Google Meet",
            VideoConferenceProvider.WebexMeetings => "Webex Meetings",
            VideoConferenceProvider.Custom => "Custom Meeting Link",
            _ => "Unknown Provider"
        };
    }

    public string GetProviderIcon()
    {
        return Provider switch
        {
            VideoConferenceProvider.Zoom => "videocam",
            VideoConferenceProvider.MicrosoftTeams => "groups",
            VideoConferenceProvider.GoogleMeet => "video_call",
            VideoConferenceProvider.WebexMeetings => "videocam",
            VideoConferenceProvider.Custom => "link",
            _ => "videocam"
        };
    }

    public bool HasDialIn()
    {
        return !string.IsNullOrWhiteSpace(DialInNumbers);
    }

    public List<DialInNumber> GetDialInNumbers()
    {
        if (string.IsNullOrWhiteSpace(DialInNumbers))
            return new List<DialInNumber>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<DialInNumber>>(DialInNumbers)
                   ?? new List<DialInNumber>();
        }
        catch
        {
            return new List<DialInNumber>();
        }
    }

    public void SetDialInNumbers(List<DialInNumber> numbers)
    {
        DialInNumbers = System.Text.Json.JsonSerializer.Serialize(numbers);
    }
}

public class DialInNumber
{
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Type { get; set; } = "Toll"; // Toll, Toll-free
}

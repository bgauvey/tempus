using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendMeetingUpdateAsync(Event originalEvent, Event updatedEvent, string organizerName, MeetingUpdateType updateType)
    {
        if (originalEvent.EventType != Core.Enums.EventType.Meeting || !originalEvent.Attendees.Any())
        {
            return; // Only send updates for meetings with attendees
        }

        _logger.LogInformation(
            "Sending meeting update notification: {UpdateType} for event {EventTitle} to {AttendeeCount} attendees",
            updateType, updatedEvent.Title, updatedEvent.Attendees.Count);

        foreach (var attendee in updatedEvent.Attendees)
        {
            await SendEmailAsync(
                toEmail: attendee.Email,
                toName: attendee.Name,
                subject: $"Meeting Update: {updatedEvent.Title}",
                body: GenerateMeetingUpdateBody(originalEvent, updatedEvent, organizerName, updateType)
            );
        }
    }

    public async Task SendMeetingInvitationAsync(Event meetingEvent, string organizerName)
    {
        if (meetingEvent.EventType != Core.Enums.EventType.Meeting || !meetingEvent.Attendees.Any())
        {
            return;
        }

        _logger.LogInformation(
            "Sending meeting invitation for event {EventTitle} to {AttendeeCount} attendees",
            meetingEvent.Title, meetingEvent.Attendees.Count);

        foreach (var attendee in meetingEvent.Attendees)
        {
            await SendEmailAsync(
                toEmail: attendee.Email,
                toName: attendee.Name,
                subject: $"Meeting Invitation: {meetingEvent.Title}",
                body: GenerateMeetingInvitationBody(meetingEvent, organizerName)
            );
        }
    }

    public async Task SendMeetingCancellationAsync(Event meetingEvent, string organizerName)
    {
        if (meetingEvent.EventType != Core.Enums.EventType.Meeting || !meetingEvent.Attendees.Any())
        {
            return;
        }

        _logger.LogInformation(
            "Sending meeting cancellation for event {EventTitle} to {AttendeeCount} attendees",
            meetingEvent.Title, meetingEvent.Attendees.Count);

        foreach (var attendee in meetingEvent.Attendees)
        {
            await SendEmailAsync(
                toEmail: attendee.Email,
                toName: attendee.Name,
                subject: $"Meeting Cancelled: {meetingEvent.Title}",
                body: GenerateMeetingCancellationBody(meetingEvent, organizerName)
            );
        }
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        // TODO: Implement actual email sending using SMTP, SendGrid, or other email service
        // For now, we'll just log the email that would be sent

        _logger.LogInformation(
            "EMAIL NOTIFICATION:\n" +
            "To: {ToName} <{ToEmail}>\n" +
            "Subject: {Subject}\n" +
            "Body:\n{Body}\n" +
            "---END EMAIL---",
            toName, toEmail, subject, body);

        // Simulate async operation
        await Task.CompletedTask;

        /* Example implementation with SMTP:
        using var client = new SmtpClient("smtp.example.com", 587)
        {
            Credentials = new NetworkCredential("username", "password"),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress("noreply@tempus.com", "Tempus Calendar"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail, toName));

        await client.SendMailAsync(message);
        */
    }

    private string GenerateMeetingUpdateBody(Event originalEvent, Event updatedEvent, string organizerName, MeetingUpdateType updateType)
    {
        var changes = new List<string>();

        if (originalEvent.StartTime != updatedEvent.StartTime || originalEvent.EndTime != updatedEvent.EndTime)
        {
            changes.Add($"<li><strong>Time:</strong> {FormatDateTime(originalEvent.StartTime)} - {FormatTime(originalEvent.EndTime)} → <strong>{FormatDateTime(updatedEvent.StartTime)} - {FormatTime(updatedEvent.EndTime)}</strong></li>");
        }

        if (originalEvent.Location != updatedEvent.Location)
        {
            changes.Add($"<li><strong>Location:</strong> {originalEvent.Location ?? "None"} → <strong>{updatedEvent.Location ?? "None"}</strong></li>");
        }

        if (originalEvent.Description != updatedEvent.Description)
        {
            changes.Add($"<li><strong>Description updated</strong></li>");
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 8px 8px; }}
        .event-details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .changes {{ background: #fff3cd; padding: 15px; border-radius: 8px; border-left: 4px solid #ffc107; }}
        ul {{ padding-left: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>📅 Meeting Update</h2>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p><strong>{organizerName}</strong> has updated the meeting: <strong>{updatedEvent.Title}</strong></p>

            <div class='changes'>
                <h3>What Changed:</h3>
                <ul>
                    {string.Join("\n", changes)}
                </ul>
            </div>

            <div class='event-details'>
                <h3>Updated Meeting Details:</h3>
                <p><strong>📅 Date & Time:</strong> {FormatDateTime(updatedEvent.StartTime)} - {FormatTime(updatedEvent.EndTime)}</p>
                {(!string.IsNullOrEmpty(updatedEvent.Location) ? $"<p><strong>📍 Location:</strong> {updatedEvent.Location}</p>" : "")}
                {(!string.IsNullOrEmpty(updatedEvent.Description) ? $"<p><strong>📝 Description:</strong> {updatedEvent.Description}</p>" : "")}
            </div>

            <p>Please update your calendar accordingly.</p>
            <p>Best regards,<br>Tempus Calendar System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateMeetingInvitationBody(Event meetingEvent, string organizerName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 8px 8px; }}
        .event-details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>📅 Meeting Invitation</h2>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p><strong>{organizerName}</strong> has invited you to a meeting:</p>

            <div class='event-details'>
                <h3>{meetingEvent.Title}</h3>
                <p><strong>📅 Date & Time:</strong> {FormatDateTime(meetingEvent.StartTime)} - {FormatTime(meetingEvent.EndTime)}</p>
                {(!string.IsNullOrEmpty(meetingEvent.Location) ? $"<p><strong>📍 Location:</strong> {meetingEvent.Location}</p>" : "")}
                {(!string.IsNullOrEmpty(meetingEvent.Description) ? $"<p><strong>📝 Description:</strong> {meetingEvent.Description}</p>" : "")}
                <p><strong>👥 Attendees:</strong> {meetingEvent.Attendees.Count} participant(s)</p>
            </div>

            <p>Please add this meeting to your calendar.</p>
            <p>Best regards,<br>Tempus Calendar System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateMeetingCancellationBody(Event meetingEvent, string organizerName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #e53935 0%, #c62828 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 8px 8px; }}
        .event-details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .cancellation-notice {{ background: #ffebee; padding: 15px; border-radius: 8px; border-left: 4px solid #e53935; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>❌ Meeting Cancelled</h2>
        </div>
        <div class='content'>
            <p>Hello,</p>

            <div class='cancellation-notice'>
                <strong>{organizerName}</strong> has cancelled the following meeting:
            </div>

            <div class='event-details'>
                <h3>{meetingEvent.Title}</h3>
                <p><strong>📅 Was scheduled for:</strong> {FormatDateTime(meetingEvent.StartTime)} - {FormatTime(meetingEvent.EndTime)}</p>
                {(!string.IsNullOrEmpty(meetingEvent.Location) ? $"<p><strong>📍 Location:</strong> {meetingEvent.Location}</p>" : "")}
            </div>

            <p>Please remove this meeting from your calendar.</p>
            <p>Best regards,<br>Tempus Calendar System</p>
        </div>
    </div>
</body>
</html>";
    }

    public async Task SendRSVPReminderAsync(Event meetingEvent, Attendee attendee)
    {
        _logger.LogInformation(
            "Sending RSVP reminder for event {EventTitle} to {AttendeeName} <{AttendeeEmail}>",
            meetingEvent.Title, attendee.Name, attendee.Email);

        await SendEmailAsync(
            toEmail: attendee.Email,
            toName: attendee.Name,
            subject: $"RSVP Reminder: {meetingEvent.Title}",
            body: GenerateRSVPReminderBody(meetingEvent, attendee)
        );
    }

    public async Task SendRSVPResponseNotificationAsync(Event meetingEvent, Attendee respondingAttendee, Attendee organizer)
    {
        _logger.LogInformation(
            "Notifying organizer {OrganizerName} about RSVP response from {AttendeeName} for event {EventTitle}",
            organizer.Name, respondingAttendee.Name, meetingEvent.Title);

        await SendEmailAsync(
            toEmail: organizer.Email,
            toName: organizer.Name,
            subject: $"RSVP Response: {respondingAttendee.Name} - {meetingEvent.Title}",
            body: GenerateRSVPResponseNotificationBody(meetingEvent, respondingAttendee)
        );
    }

    public async Task SendProposedTimeNotificationAsync(Event meetingEvent, Attendee attendee, ProposedTime proposedTime, Attendee organizer)
    {
        _logger.LogInformation(
            "Notifying organizer {OrganizerName} about proposed time from {AttendeeName} for event {EventTitle}",
            organizer.Name, attendee.Name, meetingEvent.Title);

        await SendEmailAsync(
            toEmail: organizer.Email,
            toName: organizer.Name,
            subject: $"Alternative Time Proposed: {meetingEvent.Title}",
            body: GenerateProposedTimeNotificationBody(meetingEvent, attendee, proposedTime)
        );
    }

    private string GenerateRSVPReminderBody(Event meetingEvent, Attendee attendee)
    {
        var rsvpDeadlineText = meetingEvent.RSVPDeadline.HasValue
            ? $"<p class='deadline'><strong>⏰ RSVP Deadline:</strong> {FormatDateTime(meetingEvent.RSVPDeadline.Value)}</p>"
            : "";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 8px 8px; }}
        .event-details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .deadline {{ background: #fff3cd; padding: 10px; border-radius: 8px; border-left: 4px solid #ffc107; }}
        .action-buttons {{ margin: 20px 0; text-align: center; }}
        .btn {{ display: inline-block; padding: 12px 24px; margin: 5px; text-decoration: none; border-radius: 6px; font-weight: bold; }}
        .btn-accept {{ background: #4caf50; color: white; }}
        .btn-decline {{ background: #f44336; color: white; }}
        .btn-tentative {{ background: #ff9800; color: white; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>⏰ RSVP Reminder</h2>
        </div>
        <div class='content'>
            <p>Hello {attendee.Name},</p>
            <p>This is a friendly reminder that we haven't received your response for the following meeting:</p>

            <div class='event-details'>
                <h3>{meetingEvent.Title}</h3>
                <p><strong>📅 Date & Time:</strong> {FormatDateTime(meetingEvent.StartTime)} - {FormatTime(meetingEvent.EndTime)}</p>
                {(!string.IsNullOrEmpty(meetingEvent.Location) ? $"<p><strong>📍 Location:</strong> {meetingEvent.Location}</p>" : "")}
                {(!string.IsNullOrEmpty(meetingEvent.Description) ? $"<p><strong>📝 Description:</strong> {meetingEvent.Description}</p>" : "")}
            </div>

            {rsvpDeadlineText}

            <p>Please let us know if you can attend by responding to this invitation.</p>

            <p>Best regards,<br>Tempus Calendar System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateRSVPResponseNotificationBody(Event meetingEvent, Attendee respondingAttendee)
    {
        var statusColor = respondingAttendee.Status switch
        {
            AttendeeStatus.Accepted => "#4caf50",
            AttendeeStatus.Declined => "#f44336",
            AttendeeStatus.Tentative => "#ff9800",
            _ => "#757575"
        };

        var statusIcon = respondingAttendee.Status switch
        {
            AttendeeStatus.Accepted => "✅",
            AttendeeStatus.Declined => "❌",
            AttendeeStatus.Tentative => "❓",
            _ => "⏳"
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 8px 8px; }}
        .event-details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .response-box {{ background: {statusColor}15; padding: 15px; border-radius: 8px; border-left: 4px solid {statusColor}; margin: 15px 0; }}
        .notes {{ background: #e3f2fd; padding: 15px; border-radius: 8px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>📬 RSVP Response Received</h2>
        </div>
        <div class='content'>
            <p>Hello,</p>

            <div class='response-box'>
                <h3>{statusIcon} {respondingAttendee.Name} has {respondingAttendee.Status.ToString().ToLower()} your invitation</h3>
                <p><strong>Response Date:</strong> {FormatDateTime(respondingAttendee.ResponseDate ?? DateTime.UtcNow)}</p>
            </div>

            {(!string.IsNullOrEmpty(respondingAttendee.ResponseNotes) ? $@"
            <div class='notes'>
                <h4>📝 Message from {respondingAttendee.Name}:</h4>
                <p>{respondingAttendee.ResponseNotes}</p>
            </div>" : "")}

            <div class='event-details'>
                <h3>Meeting Details:</h3>
                <p><strong>📋 Title:</strong> {meetingEvent.Title}</p>
                <p><strong>📅 Date & Time:</strong> {FormatDateTime(meetingEvent.StartTime)} - {FormatTime(meetingEvent.EndTime)}</p>
                {(!string.IsNullOrEmpty(meetingEvent.Location) ? $"<p><strong>📍 Location:</strong> {meetingEvent.Location}</p>" : "")}
            </div>

            <p>You can view all RSVP responses in your Tempus calendar.</p>
            <p>Best regards,<br>Tempus Calendar System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateProposedTimeNotificationBody(Event meetingEvent, Attendee attendee, ProposedTime proposedTime)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #2196f3 0%, #1976d2 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 8px 8px; }}
        .event-details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .proposed-time {{ background: #e3f2fd; padding: 15px; border-radius: 8px; border-left: 4px solid #2196f3; margin: 15px 0; }}
        .reason {{ background: #fff3cd; padding: 15px; border-radius: 8px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🕐 Alternative Time Proposed</h2>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p><strong>{attendee.Name}</strong> has proposed an alternative time for your meeting:</p>

            <div class='event-details'>
                <h3>Original Meeting:</h3>
                <p><strong>📋 Title:</strong> {meetingEvent.Title}</p>
                <p><strong>📅 Date & Time:</strong> {FormatDateTime(meetingEvent.StartTime)} - {FormatTime(meetingEvent.EndTime)}</p>
                {(!string.IsNullOrEmpty(meetingEvent.Location) ? $"<p><strong>📍 Location:</strong> {meetingEvent.Location}</p>" : "")}
            </div>

            <div class='proposed-time'>
                <h3>💡 Proposed Alternative Time:</h3>
                <p><strong>📅 New Date & Time:</strong> {FormatDateTime(proposedTime.ProposedStartTime)} - {FormatTime(proposedTime.ProposedEndTime)}</p>
                <p><strong>Proposed by:</strong> {attendee.Name}</p>
            </div>

            {(!string.IsNullOrEmpty(proposedTime.Reason) ? $@"
            <div class='reason'>
                <h4>📝 Reason:</h4>
                <p>{proposedTime.Reason}</p>
            </div>" : "")}

            <p>You can view this proposed time in your Tempus calendar and decide whether to accept it or keep the original time.</p>
            <p>Other attendees can also vote on this proposed time.</p>

            <p>Best regards,<br>Tempus Calendar System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string FormatDateTime(DateTime dt)
    {
        return dt.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt");
    }

    private string FormatTime(DateTime dt)
    {
        return dt.ToString("h:mm tt");
    }
}

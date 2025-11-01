using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radzen;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Web.Components.Dialogs;

namespace Tempus.Web.Helpers;

/// <summary>
/// Manages event operations for the Calendar component
/// </summary>
public class CalendarEventManager
{
    private readonly IEventRepository _eventRepository;
    private readonly DialogService _dialogService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public CalendarEventManager(
        IEventRepository eventRepository,
        DialogService dialogService,
        IEmailNotificationService emailNotificationService,
        AuthenticationStateProvider authStateProvider,
        UserManager<ApplicationUser> userManager)
    {
        _eventRepository = eventRepository;
        _dialogService = dialogService;
        _emailNotificationService = emailNotificationService;
        _authStateProvider = authStateProvider;
        _userManager = userManager;
    }

    public async Task<bool> OpenEventDialogAsync(Guid? eventId, DateTime? prefilledDate = null)
    {
        var parameters = new Dictionary<string, object>();

        if (eventId.HasValue)
        {
            parameters.Add("EventId", eventId.Value);
        }

        if (prefilledDate.HasValue)
        {
            parameters.Add("PrefilledDate", prefilledDate.Value);
        }

        var result = await _dialogService.OpenAsync<EventFormDialog>(
            eventId.HasValue ? "Edit Event" : "Create Event",
            parameters,
            new DialogOptions
            {
                Width = "600px",
                Height = "auto",
                Resizable = true,
                Draggable = true
            });

        return result is bool saved && saved;
    }

    public async Task<bool> EditSingleOccurrenceAsync(Event instance)
    {
        if (!instance.RecurrenceParentId.HasValue) return false;

        var parameters = new Dictionary<string, object>
        {
            { "EditSingleOccurrence", true },
            { "InstanceEvent", instance }
        };

        var result = await _dialogService.OpenAsync<EventFormDialog>(
            "Edit This Occurrence",
            parameters,
            new DialogOptions
            {
                Width = "600px",
                Height = "auto",
                Resizable = true,
                Draggable = true
            });

        return result is bool saved && saved;
    }

    public async Task<bool> DeleteSingleOccurrenceAsync(Event instance, string userId)
    {
        if (!instance.RecurrenceParentId.HasValue) return false;

        var result = await _dialogService.Confirm(
            "Are you sure you want to delete this occurrence?",
            "Delete Occurrence",
            new ConfirmOptions() { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (result == true)
        {
            var exceptionEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "(Deleted)",
                StartTime = instance.StartTime,
                EndTime = instance.EndTime,
                UserId = userId,
                IsRecurring = false,
                IsRecurrenceException = true,
                RecurrenceExceptionDate = instance.StartTime.Date,
                RecurrenceParentId = instance.RecurrenceParentId.Value,
                CreatedAt = DateTime.UtcNow,
                Tags = new List<string>(),
                Attendees = new List<Attendee>()
            };

            await _eventRepository.CreateAsync(exceptionEvent);
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteEventAsync(Guid id, string userId)
    {
        var result = await _dialogService.Confirm(
            "Are you sure you want to delete this event?",
            "Delete Event",
            new ConfirmOptions() { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (result == true)
        {
            await _eventRepository.DeleteAsync(id, userId);
            return true;
        }

        return false;
    }

    public async Task<Event> CreateFromTemplateAsync(string templateName, int durationMinutes, DateTime selectedDate, CalendarSettings? settings, string userId)
    {
        var now = DateTime.Now;
        var startTime = now;

        if (selectedDate.Date != DateTime.Today)
        {
            var workStartHour = settings?.WorkHoursStart.Hours ?? 8;
            startTime = selectedDate.Date.AddHours(workStartHour);
        }
        else
        {
            var minutes = startTime.Minute;
            var roundedMinutes = ((minutes + 14) / 15) * 15;
            startTime = startTime.Date.AddHours(startTime.Hour).AddMinutes(roundedMinutes);
        }

        var endTime = startTime.AddMinutes(durationMinutes);

        var eventType = templateName.ToLower() switch
        {
            "meeting" => EventType.Meeting,
            "break" => EventType.TimeBlock,
            _ => EventType.TimeBlock
        };

        var newEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = templateName,
            StartTime = startTime,
            EndTime = endTime,
            EventType = eventType,
            Priority = Priority.Medium,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Tags = new List<string>(),
            Attendees = new List<Attendee>()
        };

        await _eventRepository.CreateAsync(newEvent);
        return newEvent;
    }

    public async Task RescheduleEventAsync(Event evt, DateTime newStartTime, DateTime newEndTime)
    {
        var originalEvent = new Event
        {
            Id = evt.Id,
            Title = evt.Title,
            StartTime = evt.StartTime,
            EndTime = evt.EndTime,
            Location = evt.Location,
            Description = evt.Description,
            EventType = evt.EventType,
            Attendees = evt.Attendees
        };

        evt.StartTime = newStartTime;
        evt.EndTime = newEndTime;

        await _eventRepository.UpdateAsync(evt);

        if (evt.EventType == EventType.Meeting && evt.Attendees.Any())
        {
            var organizerName = await GetOrganizerNameAsync();
            await _emailNotificationService.SendMeetingUpdateAsync(
                originalEvent,
                evt,
                organizerName,
                MeetingUpdateType.Rescheduled
            );
        }
    }

    public async Task RescheduleSingleOccurrenceAsync(Event evt, DateTime newStartTime, DateTime newEndTime)
    {
        var exceptionEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = evt.Title,
            Description = evt.Description,
            StartTime = newStartTime,
            EndTime = newEndTime,
            Location = evt.Location,
            EventType = evt.EventType,
            Priority = evt.Priority,
            Color = evt.Color,
            UserId = evt.UserId,
            IsRecurring = false,
            IsRecurrenceException = true,
            RecurrenceExceptionDate = evt.StartTime.Date,
            RecurrenceParentId = evt.RecurrenceParentId ?? evt.Id,
            CreatedAt = DateTime.UtcNow,
            Tags = new List<string>(evt.Tags),
            Attendees = new List<Attendee>(evt.Attendees.Select(a => new Attendee
            {
                Name = a.Name,
                Email = a.Email
            }))
        };

        await _eventRepository.CreateAsync(exceptionEvent);

        if (evt.EventType == EventType.Meeting && evt.Attendees.Any())
        {
            var organizerName = await GetOrganizerNameAsync();
            var originalOccurrence = new Event
            {
                Title = evt.Title,
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                Location = evt.Location,
                Attendees = evt.Attendees
            };

            await _emailNotificationService.SendMeetingUpdateAsync(
                originalOccurrence,
                exceptionEvent,
                organizerName,
                MeetingUpdateType.Rescheduled
            );
        }
    }

    public async Task RescheduleAllOccurrencesAsync(Event evt, DateTime newStartTime, DateTime newEndTime, string userId)
    {
        var parentId = evt.RecurrenceParentId ?? evt.Id;
        var parentEvent = evt.RecurrenceParentId.HasValue
            ? await _eventRepository.GetByIdAsync(parentId, userId)
            : evt;

        if (parentEvent == null) return;

        var timeDiff = newStartTime - evt.StartTime;

        var originalEvent = new Event
        {
            Id = parentEvent.Id,
            Title = parentEvent.Title,
            StartTime = parentEvent.StartTime,
            EndTime = parentEvent.EndTime,
            Location = parentEvent.Location,
            Description = parentEvent.Description,
            EventType = parentEvent.EventType,
            Attendees = parentEvent.Attendees
        };

        parentEvent.StartTime = parentEvent.StartTime.Add(timeDiff);
        parentEvent.EndTime = parentEvent.EndTime.Add(timeDiff);

        await _eventRepository.UpdateAsync(parentEvent);

        if (parentEvent.EventType == EventType.Meeting && parentEvent.Attendees.Any())
        {
            var organizerName = await GetOrganizerNameAsync();
            await _emailNotificationService.SendMeetingUpdateAsync(
                originalEvent,
                parentEvent,
                organizerName,
                MeetingUpdateType.Rescheduled
            );
        }
    }

    private async Task<string> GetOrganizerNameAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var appUser = await _userManager.GetUserAsync(user);
        var organizerName = appUser != null ? $"{appUser.FirstName} {appUser.LastName}".Trim() : "Unknown";
        if (string.IsNullOrWhiteSpace(organizerName)) organizerName = appUser?.Email ?? "Unknown";
        return organizerName;
    }
}

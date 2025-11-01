using Microsoft.JSInterop;
using Radzen;
using Tempus.Core.Models;

namespace Tempus.Web.Helpers;

/// <summary>
/// Manages drag and drop operations for the Calendar component
/// </summary>
public class CalendarDragDropManager
{
    private readonly IJSRuntime _jsRuntime;
    private readonly DialogService _dialogService;
    private readonly CalendarEventManager _eventManager;

    public CalendarDragDropManager(
        IJSRuntime jsRuntime,
        DialogService dialogService,
        CalendarEventManager eventManager)
    {
        _jsRuntime = jsRuntime;
        _dialogService = dialogService;
        _eventManager = eventManager;
    }

    public async Task InitializeDragDropAsync<T>(DotNetObjectReference<T> dotNetHelper) where T : class
    {
        await _jsRuntime.InvokeVoidAsync("TempusCalendar.initializeDragDrop", dotNetHelper);
    }

    public async Task SetupDragDropAsync()
    {
        await _jsRuntime.InvokeVoidAsync("TempusCalendar.makeAllEventsDraggable");
        await _jsRuntime.InvokeVoidAsync("TempusCalendar.setupDropZones");
    }

    public async Task<bool?> ShowRescheduleConfirmationDialogAsync(Event evt, DateTime newStartTime, DateTime newEndTime, CalendarFormatter formatter)
    {
        var result = await _dialogService.Confirm(
            $"Do you want to reschedule all occurrences or just this one?\n\nNew time: {formatter.FormatDateTime(newStartTime)} - {formatter.FormatTime(newEndTime)}",
            "Reschedule Recurring Event",
            new ConfirmOptions
            {
                OkButtonText = "This occurrence only",
                CancelButtonText = "Cancel",
                Width = "500px"
            });

        if (result == null) return null;

        var allOccurrences = await _dialogService.Confirm(
            "Reschedule all occurrences of this event?",
            "Confirm",
            new ConfirmOptions
            {
                OkButtonText = "All occurrences",
                CancelButtonText = "This occurrence only",
                Width = "400px"
            });

        return allOccurrences ?? false;
    }

    public DateTime CalculateNewDate(DateTime selectedDate, string currentView, int dayIndex, List<DateTime> weekDays, List<DateTime> workWeekDays)
    {
        DateTime newDate = selectedDate.Date;

        if (currentView == "Week" || currentView == "WorkWeek")
        {
            var days = currentView == "Week" ? weekDays : workWeekDays;
            if (dayIndex >= 0 && dayIndex < days.Count)
            {
                newDate = days[dayIndex].Date;
            }
        }

        return newDate;
    }

    public async Task HandleEventDropAsync(
        Event evt,
        DateTime newStartTime,
        DateTime newEndTime,
        string userId,
        Func<Task> refreshCallback)
    {
        bool isRecurringInstance = evt.RecurrenceParentId.HasValue;
        bool isRecurringParent = evt.IsRecurring && !evt.RecurrenceParentId.HasValue;

        if (isRecurringInstance || isRecurringParent)
        {
            var formatter = new CalendarFormatter(null);
            var result = await ShowRescheduleConfirmationDialogAsync(evt, newStartTime, newEndTime, formatter);

            if (result == null) return;

            if (result.Value)
            {
                await _eventManager.RescheduleAllOccurrencesAsync(evt, newStartTime, newEndTime, userId);
            }
            else
            {
                await _eventManager.RescheduleSingleOccurrenceAsync(evt, newStartTime, newEndTime);
            }
        }
        else
        {
            await _eventManager.RescheduleEventAsync(evt, newStartTime, newEndTime);
        }

        await refreshCallback();
    }
}

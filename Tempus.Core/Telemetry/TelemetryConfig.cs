using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Tempus.Core.Telemetry;

/// <summary>
/// Centralized telemetry configuration for OpenTelemetry instrumentation
/// Contains all Activity Sources and Meters used throughout the application
/// </summary>
public static class TelemetryConfig
{
    // Service information
    public const string ServiceName = "Tempus";
    public const string ServiceVersion = "1.0.0";

    // Activity Sources for distributed tracing
    public static readonly ActivitySource EventActivitySource = new(
        $"{ServiceName}.Events",
        ServiceVersion);

    public static readonly ActivitySource CalendarActivitySource = new(
        $"{ServiceName}.Calendars",
        ServiceVersion);

    public static readonly ActivitySource NotificationActivitySource = new(
        $"{ServiceName}.Notifications",
        ServiceVersion);

    public static readonly ActivitySource ImportActivitySource = new(
        $"{ServiceName}.Import",
        ServiceVersion);

    public static readonly ActivitySource AnalyticsActivitySource = new(
        $"{ServiceName}.Analytics",
        ServiceVersion);

    // Meters for custom metrics
    public static readonly Meter EventMeter = new(
        $"{ServiceName}.Events",
        ServiceVersion);

    public static readonly Meter CalendarMeter = new(
        $"{ServiceName}.Calendars",
        ServiceVersion);

    public static readonly Meter NotificationMeter = new(
        $"{ServiceName}.Notifications",
        ServiceVersion);

    public static readonly Meter ImportMeter = new(
        $"{ServiceName}.Import",
        ServiceVersion);

    // Event metrics
    public static readonly Counter<long> EventsCreatedCounter = EventMeter.CreateCounter<long>(
        "tempus.events.created",
        description: "Total number of events created");

    public static readonly Counter<long> EventsUpdatedCounter = EventMeter.CreateCounter<long>(
        "tempus.events.updated",
        description: "Total number of events updated");

    public static readonly Counter<long> EventsDeletedCounter = EventMeter.CreateCounter<long>(
        "tempus.events.deleted",
        description: "Total number of events deleted");

    public static readonly Histogram<double> EventDurationHistogram = EventMeter.CreateHistogram<double>(
        "tempus.events.duration.hours",
        unit: "hours",
        description: "Distribution of event durations in hours");

    // Calendar metrics
    public static readonly Counter<long> CalendarsCreatedCounter = CalendarMeter.CreateCounter<long>(
        "tempus.calendars.created",
        description: "Total number of calendars created");

    // Notification metrics
    public static readonly Counter<long> NotificationsSentCounter = NotificationMeter.CreateCounter<long>(
        "tempus.notifications.sent",
        description: "Total number of notifications sent");

    public static readonly Counter<long> NotificationsFailedCounter = NotificationMeter.CreateCounter<long>(
        "tempus.notifications.failed",
        description: "Total number of notifications that failed to send");

    public static readonly Histogram<double> NotificationDeliveryTimeHistogram = NotificationMeter.CreateHistogram<double>(
        "tempus.notifications.delivery.time",
        unit: "ms",
        description: "Distribution of notification delivery times in milliseconds");

    // Import metrics
    public static readonly Counter<long> ImportOperationsCounter = ImportMeter.CreateCounter<long>(
        "tempus.import.operations",
        description: "Total number of import operations");

    public static readonly Counter<long> ImportedEventsCounter = ImportMeter.CreateCounter<long>(
        "tempus.import.events",
        description: "Total number of events imported");

    public static readonly Histogram<double> ImportDurationHistogram = ImportMeter.CreateHistogram<double>(
        "tempus.import.duration",
        unit: "ms",
        description: "Distribution of import operation durations in milliseconds");
}

using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Web.Services;

/// <summary>
/// Service for managing help topics and content
/// </summary>
public class HelpService : IHelpService
{
    private readonly List<HelpTopic> _helpTopics;

    public HelpService()
    {
        _helpTopics = InitializeHelpTopics();
    }

    public Task<List<HelpTopic>> GetAllTopicsAsync()
    {
        return Task.FromResult(_helpTopics);
    }

    public Task<HelpTopic?> GetTopicByIdAsync(string topicId)
    {
        var topic = _helpTopics.FirstOrDefault(t => t.Id == topicId);
        return Task.FromResult(topic);
    }

    public Task<List<HelpTopic>> GetFrequentTopicsAsync(int count = 5)
    {
        var topics = _helpTopics
            .Where(t => t.IsFrequentlyAccessed)
            .OrderBy(t => t.DisplayOrder)
            .Take(count)
            .ToList();
        return Task.FromResult(topics);
    }

    public Task<List<HelpTopic>> SearchTopicsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(_helpTopics);

        query = query.ToLower();
        var results = _helpTopics
            .Where(t =>
                t.Title.ToLower().Contains(query) ||
                t.Content.ToLower().Contains(query) ||
                t.Tags.Any(tag => tag.ToLower().Contains(query)) ||
                t.Category.ToLower().Contains(query))
            .ToList();

        return Task.FromResult(results);
    }

    public Task<List<HelpTopic>> GetRelatedTopicsAsync(string topicId)
    {
        var topic = _helpTopics.FirstOrDefault(t => t.Id == topicId);
        if (topic == null)
            return Task.FromResult(new List<HelpTopic>());

        var relatedTopics = _helpTopics
            .Where(t => topic.RelatedTopicIds.Contains(t.Id))
            .ToList();

        return Task.FromResult(relatedTopics);
    }

    public Task<List<HelpTopic>> GetTopicsByCategoryAsync(string category)
    {
        var topics = _helpTopics
            .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(topics);
    }

    private List<HelpTopic> InitializeHelpTopics()
    {
        return new List<HelpTopic>
        {
            // Getting Started
            new HelpTopic
            {
                Id = "getting-started",
                Title = "Getting Started with Tempus",
                Category = "Getting Started",
                IsFrequentlyAccessed = true,
                DisplayOrder = 1,
                Tags = new List<string> { "introduction", "basics", "overview", "start" },
                RelatedTopicIds = new List<string> { "create-event", "calendar-views", "dashboard-overview" },
                Content = @"
                    <h3>Welcome to Tempus!</h3>
                    <p>Tempus is your comprehensive time management platform. Here's how to get started:</p>
                    <ol>
                        <li><strong>Dashboard</strong> - Your command center showing upcoming events and statistics</li>
                        <li><strong>Calendar</strong> - View and manage your events in multiple formats</li>
                        <li><strong>Create Events</strong> - Add meetings, appointments, tasks, and time blocks</li>
                        <li><strong>Integrations</strong> - Connect Google, Outlook, and Apple calendars</li>
                        <li><strong>Settings</strong> - Customize your experience</li>
                    </ol>
                    <p><strong>Quick Tips:</strong></p>
                    <ul>
                        <li>Use the <strong>Create Event</strong> button in the toolbar for quick access</li>
                        <li>Switch between calendar views using the toolbar buttons</li>
                        <li>Drag and drop events to reschedule them</li>
                        <li>Use keyboard shortcuts for faster navigation</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "create-event",
                Title = "Creating and Managing Events",
                Category = "Events",
                IsFrequentlyAccessed = true,
                DisplayOrder = 2,
                Tags = new List<string> { "create", "event", "meeting", "appointment", "task" },
                RelatedTopicIds = new List<string> { "event-types", "recurring-events", "attendees" },
                Content = @"
                    <h3>Creating Events</h3>
                    <p>Create events in several ways:</p>
                    <ul>
                        <li><strong>Toolbar Button</strong> - Click the ""Create Event"" button</li>
                        <li><strong>Calendar</strong> - Click any time slot on the calendar</li>
                        <li><strong>Dashboard</strong> - Use the ""Create Event"" button on the dashboard</li>
                    </ul>

                    <h4>Event Fields</h4>
                    <ul>
                        <li><strong>Title</strong> - Name of your event (required)</li>
                        <li><strong>Start/End Time</strong> - When the event occurs</li>
                        <li><strong>Event Type</strong> - Meeting, Appointment, Task, Time Block, Reminder, or Deadline</li>
                        <li><strong>Priority</strong> - Low, Medium, High, or Urgent</li>
                        <li><strong>Description</strong> - Additional details</li>
                        <li><strong>Location</strong> - Where the event takes place</li>
                        <li><strong>Attendees</strong> - Add people from your address book</li>
                    </ul>

                    <h4>Quick Actions</h4>
                    <ul>
                        <li><strong>Right-click</strong> an event for quick Edit/Delete/Duplicate</li>
                        <li><strong>Drag and drop</strong> to reschedule</li>
                        <li><strong>Double-click</strong> to edit</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "calendar-views",
                Title = "Calendar Views and Navigation",
                Category = "Calendar",
                IsFrequentlyAccessed = true,
                DisplayOrder = 3,
                Tags = new List<string> { "calendar", "view", "month", "week", "day", "navigation" },
                RelatedTopicIds = new List<string> { "calendar-settings", "time-zones", "search-filter" },
                Content = @"
                    <h3>Calendar Views</h3>
                    <p>Tempus offers multiple calendar views to suit your workflow:</p>

                    <h4>Available Views</h4>
                    <ul>
                        <li><strong>Month View</strong> - Overview of the entire month</li>
                        <li><strong>Week View</strong> - Detailed weekly schedule with time slots</li>
                        <li><strong>Work Week</strong> - Monday through Friday focus</li>
                        <li><strong>Day View</strong> - Hour-by-hour breakdown of a single day</li>
                        <li><strong>Year View</strong> - Annual overview with event density</li>
                        <li><strong>Grid View</strong> - List format with filtering</li>
                    </ul>

                    <h4>Navigation</h4>
                    <ul>
                        <li><strong>Today Button</strong> - Jump to current date</li>
                        <li><strong>Arrow Buttons</strong> - Move forward/backward</li>
                        <li><strong>Date Picker</strong> - Jump to specific date</li>
                    </ul>

                    <h4>Features</h4>
                    <ul>
                        <li><strong>Drag and Drop</strong> - Move events to different times</li>
                        <li><strong>Quick Time Blocks</strong> - Templates for common activities</li>
                        <li><strong>Auto-scroll</strong> - Day/Week views scroll to work hours</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "integrations",
                Title = "Calendar Integrations",
                Category = "Integrations",
                IsFrequentlyAccessed = true,
                DisplayOrder = 4,
                Tags = new List<string> { "google", "outlook", "apple", "sync", "integration", "calendar" },
                RelatedTopicIds = new List<string> { "sync-events", "import-export" },
                Content = @"
                    <h3>Connect External Calendars</h3>
                    <p>Tempus supports two-way synchronization with major calendar providers:</p>

                    <h4>Supported Calendars</h4>
                    <ul>
                        <li><strong>Google Calendar</strong> - OAuth2 authentication</li>
                        <li><strong>Microsoft Outlook/Office 365</strong> - Microsoft Graph API</li>
                        <li><strong>Apple Calendar (iCloud)</strong> - CalDAV protocol</li>
                    </ul>

                    <h4>How to Connect</h4>
                    <ol>
                        <li>Go to <strong>Settings</strong> → <strong>Integrations</strong> tab</li>
                        <li>Click <strong>Connect</strong> on your desired calendar</li>
                        <li>Sign in and grant permissions</li>
                        <li>Your calendar will be connected!</li>
                    </ol>

                    <h4>Two-Way Sync</h4>
                    <p>Once connected:</p>
                    <ul>
                        <li>Events from external calendars appear in Tempus</li>
                        <li>Events created in Tempus sync to external calendars</li>
                        <li>Changes sync automatically</li>
                        <li>Click <strong>Sync Now</strong> for manual sync</li>
                    </ul>

                    <h4>Disconnecting</h4>
                    <p>Click <strong>Disconnect</strong> to stop syncing. Your events in Tempus remain unchanged.</p>"
            },

            new HelpTopic
            {
                Id = "dashboard-overview",
                Title = "Understanding Your Dashboard",
                Category = "Dashboard",
                IsFrequentlyAccessed = true,
                DisplayOrder = 5,
                Tags = new List<string> { "dashboard", "overview", "statistics", "insights" },
                RelatedTopicIds = new List<string> { "analytics", "getting-started" },
                Content = @"
                    <h3>Dashboard Overview</h3>
                    <p>Your dashboard is your command center, providing:</p>

                    <h4>Key Sections</h4>
                    <ul>
                        <li><strong>Upcoming Events</strong> - Next events on your schedule</li>
                        <li><strong>Quick Stats</strong> - Events, meetings, and hours summary</li>
                        <li><strong>Today's Schedule</strong> - Events for the current day</li>
                        <li><strong>Tasks</strong> - Pending tasks and deadlines</li>
                        <li><strong>Recent Activity</strong> - Latest changes and updates</li>
                    </ul>

                    <h4>Quick Actions</h4>
                    <ul>
                        <li><strong>Create Event</strong> - Add new events quickly</li>
                        <li><strong>View Calendar</strong> - Jump to calendar view</li>
                        <li><strong>Mark Complete</strong> - Check off completed tasks</li>
                    </ul>

                    <h4>Customization</h4>
                    <p>The dashboard adapts to your usage patterns and displays the most relevant information.</p>"
            },

            // Additional Topics
            new HelpTopic
            {
                Id = "event-types",
                Title = "Event Types and Priorities",
                Category = "Events",
                Tags = new List<string> { "types", "priority", "meeting", "task", "appointment" },
                RelatedTopicIds = new List<string> { "create-event", "recurring-events" },
                Content = @"
                    <h3>Event Types</h3>
                    <p>Tempus supports different event types for better organization:</p>
                    <ul>
                        <li><strong>Meeting</strong> - Scheduled gatherings with attendees</li>
                        <li><strong>Appointment</strong> - Personal commitments</li>
                        <li><strong>Task</strong> - To-do items with deadlines</li>
                        <li><strong>Time Block</strong> - Reserved time for focused work</li>
                        <li><strong>Reminder</strong> - Simple notifications</li>
                        <li><strong>Deadline</strong> - Important due dates</li>
                    </ul>

                    <h3>Priority Levels</h3>
                    <ul>
                        <li><strong>Low</strong> - Optional or flexible items</li>
                        <li><strong>Medium</strong> - Standard priority (default)</li>
                        <li><strong>High</strong> - Important items requiring attention</li>
                        <li><strong>Urgent</strong> - Critical items needing immediate action</li>
                    </ul>

                    <p>Events are color-coded based on priority for easy identification.</p>"
            },

            new HelpTopic
            {
                Id = "recurring-events",
                Title = "Recurring Events",
                Category = "Events",
                Tags = new List<string> { "recurring", "repeat", "series", "recurrence" },
                RelatedTopicIds = new List<string> { "create-event", "event-types" },
                Content = @"
                    <h3>Creating Recurring Events</h3>
                    <p>Set up events that repeat on a schedule:</p>

                    <h4>Recurrence Patterns</h4>
                    <ul>
                        <li><strong>Daily</strong> - Repeat every day or every N days</li>
                        <li><strong>Weekly</strong> - Repeat on specific days of the week</li>
                        <li><strong>Monthly</strong> - Repeat on specific day of month</li>
                        <li><strong>Yearly</strong> - Annual events</li>
                    </ul>

                    <h4>Editing Recurring Events</h4>
                    <p>When editing a recurring event, you can choose:</p>
                    <ul>
                        <li><strong>This Event</strong> - Change only this occurrence</li>
                        <li><strong>All Events</strong> - Update the entire series</li>
                    </ul>

                    <h4>End Date</h4>
                    <p>Specify when the recurrence should stop:</p>
                    <ul>
                        <li>Never (continues indefinitely)</li>
                        <li>After N occurrences</li>
                        <li>On a specific date</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "attendees",
                Title = "Managing Attendees",
                Category = "Events",
                Tags = new List<string> { "attendees", "contacts", "meeting", "participants" },
                RelatedTopicIds = new List<string> { "create-event", "address-book", "meeting-costs" },
                Content = @"
                    <h3>Adding Attendees</h3>
                    <p>For meeting-type events, you can add attendees:</p>

                    <h4>How to Add Attendees</h4>
                    <ol>
                        <li>Create or edit a Meeting event</li>
                        <li>Go to the <strong>Attendees</strong> tab</li>
                        <li>Search for contacts or add new ones</li>
                        <li>The current user is automatically added as organizer</li>
                    </ol>

                    <h4>Organizer</h4>
                    <ul>
                        <li>The organizer is automatically set to you</li>
                        <li>Organizers cannot be removed from attendee list</li>
                        <li>Organizers have special permissions</li>
                    </ul>

                    <h4>Auto-Save Contacts</h4>
                    <p>When you add a new attendee, they're automatically saved to your address book for future use.</p>"
            },

            new HelpTopic
            {
                Id = "time-zones",
                Title = "Working with Time Zones",
                Category = "Calendar",
                Tags = new List<string> { "timezone", "time zone", "international", "multi-location" },
                RelatedTopicIds = new List<string> { "create-event", "calendar-settings" },
                Content = @"
                    <h3>Time Zone Support</h3>
                    <p>Tempus makes it easy to coordinate across time zones:</p>

                    <h4>Setting Event Time Zones</h4>
                    <ol>
                        <li>When creating/editing an event, find the <strong>Time Zone</strong> field</li>
                        <li>Select from the dropdown (includes quick-select common zones)</li>
                        <li>The event stores its original time zone</li>
                    </ol>

                    <h4>How It Works</h4>
                    <ul>
                        <li>Events display in <strong>your</strong> local time zone</li>
                        <li>Events in different zones show a 🌍 indicator</li>
                        <li>Original time zone is preserved for all participants</li>
                        <li>Automatic daylight saving time adjustment</li>
                    </ul>

                    <h4>Example</h4>
                    <p>A meeting set for 2:00 PM EST shows as 11:00 AM PST in your calendar if you're on the West Coast.</p>

                    <h4>Default Time Zone</h4>
                    <p>Set your default time zone in <strong>Settings</strong> → <strong>General</strong> → <strong>Time Zone</strong></p>"
            },

            new HelpTopic
            {
                Id = "search-filter",
                Title = "Search and Filter Events",
                Category = "Calendar",
                Tags = new List<string> { "search", "filter", "find", "advanced search" },
                RelatedTopicIds = new List<string> { "calendar-views", "bulk-operations" },
                Content = @"
                    <h3>Advanced Search</h3>
                    <p>Find events quickly using powerful search filters:</p>

                    <h4>Opening Search</h4>
                    <p>Click the <strong>Advanced Search</strong> button in the calendar toolbar.</p>

                    <h4>Search Options</h4>
                    <ul>
                        <li><strong>Text Search</strong> - Search in title, description, location, attendees</li>
                        <li><strong>Date Range</strong> - Filter by start/end dates</li>
                        <li><strong>Event Types</strong> - Filter by Meeting, Task, etc.</li>
                        <li><strong>Priorities</strong> - Filter by Low, Medium, High, Urgent</li>
                        <li><strong>Status</strong> - All, Completed, or Incomplete</li>
                        <li><strong>Time of Day</strong> - Find morning/afternoon/evening events</li>
                    </ul>

                    <h4>Sorting Results</h4>
                    <p>Sort by:</p>
                    <ul>
                        <li>Start Time (default)</li>
                        <li>End Time</li>
                        <li>Title</li>
                        <li>Priority</li>
                        <li>Created Date</li>
                        <li>Updated Date</li>
                    </ul>

                    <h4>Clear Search</h4>
                    <p>Click <strong>Clear Search</strong> in the calendar header to return to full calendar view.</p>"
            },

            new HelpTopic
            {
                Id = "bulk-operations",
                Title = "Bulk Event Operations",
                Category = "Calendar",
                Tags = new List<string> { "bulk", "multiple", "batch", "select", "mass edit" },
                RelatedTopicIds = new List<string> { "create-event", "search-filter" },
                Content = @"
                    <h3>Bulk Operations</h3>
                    <p>Manage multiple events at once for efficiency:</p>

                    <h4>Entering Selection Mode</h4>
                    <ol>
                        <li>Click <strong>Select Events</strong> in the calendar toolbar</li>
                        <li>Click events to select them (golden border appears)</li>
                        <li>Selection count shows in the bulk toolbar</li>
                    </ol>

                    <h4>Available Operations</h4>
                    <ul>
                        <li><strong>Change Type</strong> - Convert to Meeting, Task, etc.</li>
                        <li><strong>Set Priority</strong> - Update priority level</li>
                        <li><strong>Set Color</strong> - Apply color to selected events</li>
                        <li><strong>Move Events</strong> - Shift by days/hours/minutes</li>
                        <li><strong>Mark Complete</strong> - Complete multiple tasks</li>
                        <li><strong>Mark Incomplete</strong> - Reset completion status</li>
                        <li><strong>Delete</strong> - Remove multiple events (with confirmation)</li>
                    </ul>

                    <h4>Tips</h4>
                    <ul>
                        <li>Use right-click menu to select/deselect</li>
                        <li>Clear selection anytime with the <strong>Clear</strong> button</li>
                        <li>Exit selection mode by clicking <strong>Exit Selection</strong></li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "analytics",
                Title = "Calendar Analytics and Insights",
                Category = "Analytics",
                Tags = new List<string> { "analytics", "insights", "statistics", "reports", "trends" },
                RelatedTopicIds = new List<string> { "dashboard-overview", "benchmarks" },
                Content = @"
                    <h3>Calendar Analytics</h3>
                    <p>Gain insights into your time management:</p>

                    <h4>Key Metrics</h4>
                    <ul>
                        <li><strong>Calendar Health Score</strong> - 0-100 rating of schedule quality</li>
                        <li><strong>Time Usage Breakdown</strong> - By event type, day, hour</li>
                        <li><strong>Meeting Analytics</strong> - Duration, costs, participants</li>
                        <li><strong>Productivity Metrics</strong> - Focus time, completion rate</li>
                        <li><strong>AI Recommendations</strong> - Smart suggestions</li>
                    </ul>

                    <h4>Analysis Periods</h4>
                    <p>View analytics for:</p>
                    <ul>
                        <li>Last 7 Days</li>
                        <li>Last 30 Days</li>
                        <li>Last 90 Days</li>
                        <li>Last Year</li>
                    </ul>

                    <h4>Export Reports</h4>
                    <p>Download analytics in PDF, CSV, or Excel format.</p>

                    <h4>Interactive Charts</h4>
                    <ul>
                        <li>Donut charts for time distribution</li>
                        <li>Column charts for daily activity</li>
                        <li>Heatmaps for peak hours</li>
                        <li>Trend lines for forecasting</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "benchmarks",
                Title = "Industry Benchmarks",
                Category = "Analytics",
                Tags = new List<string> { "benchmarks", "standards", "comparison", "best practices" },
                RelatedTopicIds = new List<string> { "analytics", "meeting-costs" },
                Content = @"
                    <h3>Industry Benchmarks</h3>
                    <p>Compare your time management against industry standards:</p>

                    <h4>Benchmark Categories</h4>
                    <ul>
                        <li><strong>Meetings</strong> - Time percentage, duration, size</li>
                        <li><strong>Focus Time</strong> - Percentage and block duration</li>
                        <li><strong>Work Hours</strong> - Weekly and daily averages</li>
                        <li><strong>Task Management</strong> - Completion rate</li>
                        <li><strong>Schedule Quality</strong> - Fragmentation score</li>
                    </ul>

                    <h4>Status Indicators</h4>
                    <ul>
                        <li>🟢 <strong>Excellent</strong> - Meeting or exceeding standards</li>
                        <li>🔵 <strong>At Standard</strong> - Within acceptable range</li>
                        <li>🟠 <strong>Near Standard</strong> - Needs attention</li>
                        <li>🔴 <strong>Below Standard</strong> - Requires improvement</li>
                    </ul>

                    <h4>Top Recommendations</h4>
                    <p>Get prioritized recommendations based on your worst-performing metrics.</p>

                    <h4>Analysis Periods</h4>
                    <p>Compare over 7, 30, or 90 days.</p>"
            },

            new HelpTopic
            {
                Id = "import-export",
                Title = "Import and Export",
                Category = "Data",
                Tags = new List<string> { "import", "export", "ics", "pst", "pdf" },
                RelatedTopicIds = new List<string> { "integrations", "sync-events" },
                Content = @"
                    <h3>Import Events</h3>
                    <p>Bring events from other calendar applications:</p>

                    <h4>ICS File Import</h4>
                    <ol>
                        <li>Go to <strong>Import ICS</strong> from the sidebar</li>
                        <li>Upload your .ics file (from Google, Outlook, Apple)</li>
                        <li>Preview the parsed events</li>
                        <li>Click <strong>Save All Events</strong></li>
                    </ol>

                    <h4>PST File Import</h4>
                    <p>Import calendar events from Microsoft Outlook PST files:</p>
                    <ul>
                        <li>Supports files up to 100MB</li>
                        <li>Automatically detects calendar folders</li>
                        <li>Preserves attendees and meeting details</li>
                    </ul>

                    <h3>Export Options</h3>
                    <ul>
                        <li><strong>Daily Agenda PDF</strong> - Export day schedule as PDF</li>
                        <li><strong>Analytics Reports</strong> - Export in PDF, CSV, or Excel</li>
                        <li><strong>ICS Export</strong> - Export events to standard format (coming soon)</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "calendar-settings",
                Title = "Calendar Settings and Preferences",
                Category = "Settings",
                Tags = new List<string> { "settings", "preferences", "customize", "configuration" },
                RelatedTopicIds = new List<string> { "time-zones", "notifications", "calendar-views" },
                Content = @"
                    <h3>Customizing Your Calendar</h3>
                    <p>Access settings via your profile menu (top-right avatar).</p>

                    <h4>General Settings</h4>
                    <ul>
                        <li><strong>Time Format</strong> - 12-hour or 24-hour</li>
                        <li><strong>Date Format</strong> - Various regional formats</li>
                        <li><strong>Time Zone</strong> - Your default timezone</li>
                        <li><strong>Week Start</strong> - Sunday or Monday</li>
                        <li><strong>Default Calendar View</strong> - Preferred view on startup</li>
                    </ul>

                    <h4>Work Hours</h4>
                    <ul>
                        <li><strong>Working Hours</strong> - Set your typical work schedule</li>
                        <li><strong>Lunch Break</strong> - Define break times</li>
                        <li><strong>Time Slot Duration</strong> - Calendar grid intervals</li>
                        <li><strong>Buffer Times</strong> - Gaps between meetings</li>
                    </ul>

                    <h4>Event Defaults</h4>
                    <ul>
                        <li><strong>Default Duration</strong> - New event length</li>
                        <li><strong>Default Visibility</strong> - Public or private</li>
                        <li><strong>Default Colors</strong> - Event color schemes</li>
                        <li><strong>Default Location</strong> - Common meeting places</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "notifications",
                Title = "Notifications and Reminders",
                Category = "Settings",
                Tags = new List<string> { "notifications", "reminders", "alerts", "email", "browser" },
                RelatedTopicIds = new List<string> { "calendar-settings", "create-event" },
                Content = @"
                    <h3>Notification System</h3>
                    <p>Stay informed about your events with flexible notifications:</p>

                    <h4>Email Notifications</h4>
                    <p>Receive email alerts for:</p>
                    <ul>
                        <li>Meeting created</li>
                        <li>Meeting updated</li>
                        <li>Meeting cancelled</li>
                        <li>Upcoming event reminders</li>
                    </ul>

                    <h4>Browser Notifications</h4>
                    <p>Get desktop alerts:</p>
                    <ol>
                        <li>Enable in <strong>Settings</strong> → <strong>Notifications</strong></li>
                        <li>Grant browser permission when prompted</li>
                        <li>Receive real-time desktop alerts</li>
                        <li>Works even when browser is in background</li>
                    </ol>

                    <h4>Reminder Times</h4>
                    <p>Set default reminders:</p>
                    <ul>
                        <li>5 minutes before</li>
                        <li>10 minutes before</li>
                        <li>15 minutes before</li>
                        <li>30 minutes before</li>
                        <li>1 hour before</li>
                        <li>1 day before</li>
                    </ul>

                    <h4>Per-Event Notifications</h4>
                    <p>Customize notifications for individual events when creating or editing them.</p>"
            },

            new HelpTopic
            {
                Id = "address-book",
                Title = "Address Book and Contacts",
                Category = "Contacts",
                Tags = new List<string> { "contacts", "address book", "attendees", "people" },
                RelatedTopicIds = new List<string> { "attendees", "create-event" },
                Content = @"
                    <h3>Managing Contacts</h3>
                    <p>Keep track of people you meet with:</p>

                    <h4>Adding Contacts</h4>
                    <ol>
                        <li>Go to <strong>Address Book</strong> from the sidebar</li>
                        <li>Click <strong>Add Contact</strong></li>
                        <li>Fill in contact details:
                            <ul>
                                <li>Name (required)</li>
                                <li>Email</li>
                                <li>Phone</li>
                                <li>Company</li>
                                <li>Notes</li>
                            </ul>
                        </li>
                        <li>Save the contact</li>
                    </ol>

                    <h4>Auto-Save from Events</h4>
                    <p>When you add a new attendee to a meeting, they're automatically saved to your address book.</p>

                    <h4>Using Contacts</h4>
                    <ul>
                        <li>Search when adding attendees to meetings</li>
                        <li>Quick access to email and phone</li>
                        <li>Track meeting history with each contact</li>
                    </ul>

                    <h4>Managing Contacts</h4>
                    <ul>
                        <li>Edit contact details anytime</li>
                        <li>Delete contacts (doesn't affect past events)</li>
                        <li>Search and filter your contact list</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "meeting-costs",
                Title = "Meeting Cost Calculator",
                Category = "Meetings",
                Tags = new List<string> { "meeting", "cost", "calculator", "budget", "expenses" },
                RelatedTopicIds = new List<string> { "attendees", "analytics" },
                Content = @"
                    <h3>Meeting Cost Calculator</h3>
                    <p>Understand the true cost of meetings:</p>

                    <h4>How It Works</h4>
                    <p>For meeting-type events, Tempus calculates costs based on:</p>
                    <ul>
                        <li><strong>Number of Attendees</strong> - People in the meeting</li>
                        <li><strong>Meeting Duration</strong> - Length in hours</li>
                        <li><strong>Hourly Rate</strong> - Cost per person per hour</li>
                    </ul>

                    <h4>Setting Cost</h4>
                    <ol>
                        <li>Create or edit a Meeting event</li>
                        <li>Go to <strong>Meeting Cost Calculator</strong> section</li>
                        <li>Set <strong>Hourly Cost Per Attendee</strong> (default: $75/hour)</li>
                        <li>View the real-time <strong>Estimated Meeting Cost</strong></li>
                    </ol>

                    <h4>Example</h4>
                    <p>A 2-hour meeting with 5 attendees at $75/hour = $750 total cost</p>

                    <h4>Analytics</h4>
                    <p>View total meeting costs in:</p>
                    <ul>
                        <li>Dashboard statistics</li>
                        <li>Analytics page</li>
                        <li>Exported reports</li>
                    </ul>

                    <h4>Benefits</h4>
                    <ul>
                        <li>Make informed decisions about meeting necessity</li>
                        <li>Identify costly meetings</li>
                        <li>Optimize meeting duration and attendee count</li>
                        <li>Track meeting budget over time</li>
                    </ul>"
            },

            new HelpTopic
            {
                Id = "sync-events",
                Title = "Syncing with External Calendars",
                Category = "Integrations",
                Tags = new List<string> { "sync", "synchronization", "two-way", "update" },
                RelatedTopicIds = new List<string> { "integrations", "import-export" },
                Content = @"
                    <h3>Two-Way Synchronization</h3>
                    <p>Keep your calendars in perfect sync:</p>

                    <h4>How Sync Works</h4>
                    <p>Once connected, events sync automatically:</p>
                    <ul>
                        <li><strong>From External → Tempus</strong> - Import events</li>
                        <li><strong>From Tempus → External</strong> - Export events</li>
                        <li><strong>Bidirectional</strong> - Changes sync both ways</li>
                    </ul>

                    <h4>Manual Sync</h4>
                    <ol>
                        <li>Go to <strong>Settings</strong> → <strong>Integrations</strong></li>
                        <li>Find your connected calendar</li>
                        <li>Click <strong>Sync Now</strong></li>
                        <li>Wait for sync to complete</li>
                        <li>View imported/exported count in notification</li>
                    </ol>

                    <h4>Sync Indicators</h4>
                    <ul>
                        <li><strong>Last Synced</strong> - Timestamp of last sync</li>
                        <li><strong>Progress Bar</strong> - Sync in progress</li>
                        <li><strong>Success/Error Messages</strong> - Sync status</li>
                    </ul>

                    <h4>Deduplication</h4>
                    <p>Tempus prevents duplicate events using unique IDs:</p>
                    <ul>
                        <li>Google Event IDs</li>
                        <li>Outlook Event IDs</li>
                        <li>CalDAV UIDs</li>
                    </ul>

                    <h4>Troubleshooting</h4>
                    <ul>
                        <li>If events don't sync, try clicking Sync Now</li>
                        <li>Check connection status in Integrations</li>
                        <li>Verify permissions are granted</li>
                        <li>Reconnect if authentication expires</li>
                    </ul>"
            }
        };
    }
}

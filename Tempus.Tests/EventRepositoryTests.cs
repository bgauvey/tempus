using Microsoft.EntityFrameworkCore;
using Tempus.Core.Models;
using Tempus.Core.Enums;
using Tempus.Infrastructure.Data;
using Tempus.Infrastructure.Repositories;

namespace Tempus.Tests;

public class EventRepositoryTests : IDisposable
{
    private readonly TempusDbContext _context;
    private readonly EventRepository _repository;
    private const string TestUserId = "test-user-123";

    public EventRepositoryTests()
    {
        // Create a unique database for each test to avoid conflicts
        var options = new DbContextOptionsBuilder<TempusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TempusDbContext(options);
        _repository = new EventRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task UpdateEvent_WithNewAttendee_ShouldInsertNotUpdate()
    {
        // Arrange - Create an initial event with one attendee
        var initialEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            EventType = EventType.Meeting,
            Priority = Priority.Medium,
            UserId = TestUserId,
            Attendees = new List<Attendee>
            {
                new Attendee
                {
                    Id = Guid.NewGuid(),
                    Name = "Original Attendee",
                    Email = "original@example.com",
                    IsOrganizer = true,
                    Status = AttendeeStatus.Accepted
                }
            }
        };

        await _repository.CreateAsync(initialEvent);

        // Act - Load the event (simulating what happens in the dialog)
        var loadedEvent = await _repository.GetByIdAsync(initialEvent.Id, TestUserId);
        Assert.NotNull(loadedEvent);
        Assert.Single(loadedEvent.Attendees);

        // Add a new attendee with Guid.Empty (simulating adding in the UI)
        var newAttendee = new Attendee
        {
            Id = Guid.Empty,  // This is the key - new attendees have empty GUID
            Name = "New Attendee",
            Email = "new@example.com",
            IsOrganizer = false,
            Status = AttendeeStatus.Pending,
            EventId = loadedEvent.Id
        };
        loadedEvent.Attendees.Add(newAttendee);

        // Update the event (this should INSERT the new attendee, not UPDATE)
        var updatedEvent = await _repository.UpdateAsync(loadedEvent);

        // Assert - Verify the new attendee was inserted
        Assert.NotNull(updatedEvent);
        Assert.Equal(2, updatedEvent.Attendees.Count);

        // Verify both attendees exist in the database
        var verifyEvent = await _repository.GetByIdAsync(initialEvent.Id, TestUserId);
        Assert.NotNull(verifyEvent);
        Assert.Equal(2, verifyEvent.Attendees.Count);
        Assert.Contains(verifyEvent.Attendees, a => a.Email == "original@example.com");
        Assert.Contains(verifyEvent.Attendees, a => a.Email == "new@example.com");

        // Verify the new attendee got a real GUID
        var insertedAttendee = verifyEvent.Attendees.First(a => a.Email == "new@example.com");
        Assert.NotEqual(Guid.Empty, insertedAttendee.Id);
    }
}

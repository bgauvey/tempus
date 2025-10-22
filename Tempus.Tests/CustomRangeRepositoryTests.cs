using Microsoft.EntityFrameworkCore;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;
using Tempus.Infrastructure.Repositories;

namespace Tempus.Tests;

public class CustomRangeRepositoryTests : IDisposable
{
    private readonly TempusDbContext _context;
    private readonly CustomRangeRepository _repository;
    private const string TestUserId = "test-user-123";

    public CustomRangeRepositoryTests()
    {
        // Create a unique database for each test to avoid conflicts
        var options = new DbContextOptionsBuilder<TempusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TempusDbContext(options);
        _repository = new CustomRangeRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveCustomRangeSuccessfully()
    {
        // Arrange
        var range = new CustomCalendarRange
        {
            Id = Guid.NewGuid(),
            Name = "Test Range",
            DaysCount = 30,
            ShowWeekends = true,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(range);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(range.Id, result.Id);
        Assert.Equal(range.Name, result.Name);
        Assert.Equal(range.DaysCount, result.DaysCount);
        Assert.Equal(range.ShowWeekends, result.ShowWeekends);

        // Verify it's actually in the database
        var savedRange = await _context.CustomCalendarRanges.FindAsync(range.Id);
        Assert.NotNull(savedRange);
        Assert.Equal("Test Range", savedRange.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectRange()
    {
        // Arrange
        var range = new CustomCalendarRange
        {
            Id = Guid.NewGuid(),
            Name = "Find Me",
            DaysCount = 60,
            ShowWeekends = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(range);

        // Act
        var result = await _repository.GetByIdAsync(range.Id, TestUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(range.Id, result.Id);
        Assert.Equal("Find Me", result.Name);
        Assert.Equal(60, result.DaysCount);
        Assert.False(result.ShowWeekends);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForNonExistentId()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId, TestUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRangesOrderedByName()
    {
        // Arrange
        var ranges = new[]
        {
            new CustomCalendarRange
            {
                Id = Guid.NewGuid(),
                Name = "Z Range",
                DaysCount = 10,
                ShowWeekends = true,
                UserId = TestUserId,
                CreatedAt = DateTime.UtcNow
            },
            new CustomCalendarRange
            {
                Id = Guid.NewGuid(),
                Name = "A Range",
                DaysCount = 20,
                ShowWeekends = false,
                UserId = TestUserId,
                CreatedAt = DateTime.UtcNow
            },
            new CustomCalendarRange
            {
                Id = Guid.NewGuid(),
                Name = "M Range",
                DaysCount = 30,
                ShowWeekends = true,
                UserId = TestUserId,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var range in ranges)
        {
            await _repository.CreateAsync(range);
        }

        // Act
        var result = await _repository.GetAllAsync(TestUserId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("A Range", result[0].Name);
        Assert.Equal("M Range", result[1].Name);
        Assert.Equal("Z Range", result[2].Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyListWhenNoRanges()
    {
        // Act
        var result = await _repository.GetAllAsync(TestUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRangeSuccessfully()
    {
        // Arrange
        var range = new CustomCalendarRange
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            DaysCount = 40,
            ShowWeekends = true,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(range);

        // Act
        range.Name = "Updated Name";
        range.DaysCount = 50;
        range.ShowWeekends = false;
        var result = await _repository.UpdateAsync(range);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(50, result.DaysCount);
        Assert.False(result.ShowWeekends);

        // Verify the change persisted
        var updatedRange = await _repository.GetByIdAsync(range.Id, TestUserId);
        Assert.NotNull(updatedRange);
        Assert.Equal("Updated Name", updatedRange.Name);
        Assert.Equal(50, updatedRange.DaysCount);
        Assert.False(updatedRange.ShowWeekends);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRangeSuccessfully()
    {
        // Arrange
        var range = new CustomCalendarRange
        {
            Id = Guid.NewGuid(),
            Name = "To Be Deleted",
            DaysCount = 15,
            ShowWeekends = true,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(range);

        // Verify it exists
        var existingRange = await _repository.GetByIdAsync(range.Id, TestUserId);
        Assert.NotNull(existingRange);

        // Act
        await _repository.DeleteAsync(range.Id, TestUserId);

        // Assert
        var deletedRange = await _repository.GetByIdAsync(range.Id, TestUserId);
        Assert.Null(deletedRange);

        // Verify it's not in the database
        var count = await _context.CustomCalendarRanges.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task DeleteAsync_ShouldHandleNonExistentId()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act - should not throw
        await _repository.DeleteAsync(nonExistentId, TestUserId);

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public async Task CreateMultipleRanges_ShouldAllPersist()
    {
        // Arrange & Act
        var range1 = await _repository.CreateAsync(new CustomCalendarRange
        {
            Id = Guid.NewGuid(),
            Name = "Range 1",
            DaysCount = 10,
            ShowWeekends = true,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow
        });

        var range2 = await _repository.CreateAsync(new CustomCalendarRange
        {
            Id = Guid.NewGuid(),
            Name = "Range 2",
            DaysCount = 20,
            ShowWeekends = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow
        });

        // Assert
        var allRanges = await _repository.GetAllAsync(TestUserId);
        Assert.Equal(2, allRanges.Count);
        Assert.Contains(allRanges, r => r.Name == "Range 1");
        Assert.Contains(allRanges, r => r.Name == "Range 2");
    }

    [Fact]
    public async Task Create_Update_Delete_FullLifecycle_ShouldWork()
    {
        // Create
        var range = new CustomCalendarRange
        {
            Id = Guid.NewGuid(),
            Name = "Lifecycle Test",
            DaysCount = 25,
            ShowWeekends = true,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repository.CreateAsync(range);
        Assert.NotNull(created);
        Assert.Equal("Lifecycle Test", created.Name);

        // Update
        created.Name = "Updated Lifecycle";
        created.DaysCount = 35;
        var updated = await _repository.UpdateAsync(created);
        Assert.Equal("Updated Lifecycle", updated.Name);
        Assert.Equal(35, updated.DaysCount);

        // Verify update persisted
        var retrieved = await _repository.GetByIdAsync(created.Id, TestUserId);
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Lifecycle", retrieved.Name);

        // Delete
        await _repository.DeleteAsync(created.Id, TestUserId);
        var deleted = await _repository.GetByIdAsync(created.Id, TestUserId);
        Assert.Null(deleted);
    }
}

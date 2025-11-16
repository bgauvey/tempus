using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class BookingPageRepository : IBookingPageRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<BookingPageRepository> _logger;

    public BookingPageRepository(IDbContextFactory<TempusDbContext> contextFactory, ILogger<BookingPageRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<BookingPage?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BookingPages
            .Include(bp => bp.Calendar)
            .FirstOrDefaultAsync(bp => bp.Id == id && bp.UserId == userId);
    }

    public async Task<BookingPage?> GetBySlugAsync(string slug)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BookingPages
            .Include(bp => bp.Calendar)
            .Include(bp => bp.User)
            .FirstOrDefaultAsync(bp => bp.Slug == slug && bp.IsActive);
    }

    public async Task<List<BookingPage>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BookingPages
            .Include(bp => bp.Calendar)
            .Where(bp => bp.UserId == userId)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BookingPage>> GetActiveBookingPagesAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BookingPages
            .Include(bp => bp.Calendar)
            .Where(bp => bp.UserId == userId && bp.IsActive)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();
    }

    public async Task<BookingPage> CreateAsync(BookingPage bookingPage)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Ensure slug is unique
        if (await SlugExistsAsync(bookingPage.Slug))
        {
            throw new InvalidOperationException($"A booking page with the slug '{bookingPage.Slug}' already exists.");
        }

        context.BookingPages.Add(bookingPage);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created booking page {BookingPageId} with slug {Slug}", bookingPage.Id, bookingPage.Slug);
        return bookingPage;
    }

    public async Task<BookingPage> UpdateAsync(BookingPage bookingPage)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Ensure slug is unique (excluding current booking page)
        if (await SlugExistsAsync(bookingPage.Slug, bookingPage.Id))
        {
            throw new InvalidOperationException($"A booking page with the slug '{bookingPage.Slug}' already exists.");
        }

        context.BookingPages.Update(bookingPage);
        bookingPage.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        _logger.LogInformation("Updated booking page {BookingPageId}", bookingPage.Id);
        return bookingPage;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var bookingPage = await context.BookingPages
            .FirstOrDefaultAsync(bp => bp.Id == id && bp.UserId == userId);

        if (bookingPage != null)
        {
            context.BookingPages.Remove(bookingPage);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted booking page {BookingPageId} with slug {Slug}", id, bookingPage.Slug);
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.BookingPages.Where(bp => bp.Slug == slug);

        if (excludeId.HasValue)
        {
            query = query.Where(bp => bp.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task IncrementBookingCountAsync(Guid bookingPageId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var bookingPage = await context.BookingPages.FindAsync(bookingPageId);

        if (bookingPage != null)
        {
            bookingPage.TotalBookings++;
            bookingPage.LastBookingAt = DateTime.UtcNow;
            bookingPage.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            _logger.LogDebug("Incremented booking count for booking page {BookingPageId} to {Count}",
                bookingPageId, bookingPage.TotalBookings);
        }
    }
}

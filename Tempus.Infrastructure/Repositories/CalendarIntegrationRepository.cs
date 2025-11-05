using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class CalendarIntegrationRepository : ICalendarIntegrationRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;

    public CalendarIntegrationRepository(IDbContextFactory<TempusDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<CalendarIntegration?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.Id == id);
    }

    public async Task<List<CalendarIntegration>> GetAllByUserIdAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CalendarIntegrations
            .Where(ci => ci.UserId == userId)
            .OrderByDescending(ci => ci.CreatedAt)
            .ToListAsync();
    }

    public async Task<CalendarIntegration?> GetByProviderAsync(string userId, string provider)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == provider);
    }

    public async Task<CalendarIntegration> CreateAsync(CalendarIntegration integration)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.CalendarIntegrations.Add(integration);
        await context.SaveChangesAsync();
        return integration;
    }

    public async Task<CalendarIntegration> UpdateAsync(CalendarIntegration integration)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.CalendarIntegrations.Update(integration);
        await context.SaveChangesAsync();
        return integration;
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var integration = await context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.Id == id);

        if (integration != null)
        {
            context.CalendarIntegrations.Remove(integration);
            await context.SaveChangesAsync();
        }
    }
}

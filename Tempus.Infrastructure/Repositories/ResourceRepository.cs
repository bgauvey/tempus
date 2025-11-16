using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<ResourceRepository> _logger;

    public ResourceRepository(IDbContextFactory<TempusDbContext> contextFactory, ILogger<ResourceRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Resource?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Resources
            .Include(r => r.Reservations)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<List<Resource>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Resources
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<Resource>> GetByTypeAsync(ResourceType type, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Resources
            .Where(r => r.UserId == userId && r.ResourceType == type)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<Resource>> GetAvailableResourcesAsync(ResourceType? type, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Resources
            .Where(r => r.UserId == userId && r.IsAvailable && r.Condition != ResourceCondition.OutOfService);

        if (type.HasValue)
        {
            query = query.Where(r => r.ResourceType == type.Value);
        }

        return await query.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Resource> CreateAsync(Resource resource)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created resource {ResourceId} with name {ResourceName}", resource.Id, resource.Name);
        return resource;
    }

    public async Task<Resource> UpdateAsync(Resource resource)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        resource.UpdatedAt = DateTime.UtcNow;
        context.Resources.Update(resource);
        await context.SaveChangesAsync();

        _logger.LogInformation("Updated resource {ResourceId}", resource.Id);
        return resource;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var resource = await context.Resources
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (resource != null)
        {
            context.Resources.Remove(resource);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted resource {ResourceId} with name {ResourceName}", id, resource.Name);
        }
    }

    public async Task<bool> ResourceExistsAsync(string name, string userId, Guid? excludeId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Resources.Where(r => r.Name == name && r.UserId == userId);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}

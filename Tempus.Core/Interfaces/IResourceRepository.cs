using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, string userId);
    Task<List<Resource>> GetAllAsync(string userId);
    Task<List<Resource>> GetByTypeAsync(ResourceType type, string userId);
    Task<List<Resource>> GetAvailableResourcesAsync(ResourceType? type, string userId);
    Task<Resource> CreateAsync(Resource resource);
    Task<Resource> UpdateAsync(Resource resource);
    Task DeleteAsync(Guid id, string userId);
    Task<bool> ResourceExistsAsync(string name, string userId, Guid? excludeId = null);
}

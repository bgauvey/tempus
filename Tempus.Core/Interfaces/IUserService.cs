using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Find a user by their email address
    /// </summary>
    Task<ApplicationUser?> FindByEmailAsync(string email);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<ApplicationUser?> FindByIdAsync(string userId);

    /// <summary>
    /// Search users by email or username
    /// </summary>
    Task<List<ApplicationUser>> SearchUsersAsync(string query, int maxResults = 10);
}

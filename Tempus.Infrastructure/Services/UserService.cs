using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<List<ApplicationUser>> SearchUsersAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<ApplicationUser>();
        }

        query = query.ToLower();

        return await _userManager.Users
            .Where(u => u.Email!.ToLower().Contains(query) ||
                       u.UserName!.ToLower().Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }
}

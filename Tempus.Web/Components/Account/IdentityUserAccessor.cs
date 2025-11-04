using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Tempus.Core.Models;

namespace Tempus.Web.Components.Account;

// This accessor creates a short-lived scope for UserManager operations so that
// a fresh DbContext is used for each call. This avoids sharing a UserManager
// (and its underlying DbContext) across concurrent Blazor Server threads.
internal sealed class IdentityUserAccessor(IdentityRedirectManager redirectManager, IServiceScopeFactory scopeFactory)
{
    public async Task<ApplicationUser?> GetRequiredUserAsync(HttpContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            var userId = userManager.GetUserId(context.User);
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userId}'.", context);
        }

        return user;
    }

    public async Task<ApplicationUser?> GetUserAsync(System.Security.Claims.ClaimsPrincipal principal)
    {
        using var scope = scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await userManager.GetUserAsync(principal);
    }
}

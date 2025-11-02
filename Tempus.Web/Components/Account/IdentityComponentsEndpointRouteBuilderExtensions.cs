using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tempus.Core.Models;

namespace Microsoft.AspNetCore.Routing;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/PerformLogin", async (
            [FromForm] string email,
            [FromForm] string password,
            [FromForm] bool? rememberMe,
            [FromForm] string? returnUrl,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var result = await signInManager.PasswordSignInAsync(email, password, rememberMe ?? false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return TypedResults.LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/dashboard" : returnUrl);
            }

            return TypedResults.Redirect($"/Account/Login?error={(result.IsLockedOut ? "locked" : "invalid")}");
        });

        accountGroup.MapPost("/PerformRegister", async (
            [FromForm] string email,
            [FromForm] string password,
            [FromForm] string firstName,
            [FromForm] string lastName,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return TypedResults.LocalRedirect("/dashboard");
            }

            var errors = string.Join(",", result.Errors.Select(e => e.Code));
            return TypedResults.Redirect($"/Account/Register?error={errors}");
        });

        accountGroup.MapPost("/Logout", async (
            ClaimsPrincipal user,
            SignInManager<ApplicationUser> signInManager,
            [FromForm] string? returnUrl) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect("~/");
        }).DisableAntiforgery();

        return accountGroup;
    }
}

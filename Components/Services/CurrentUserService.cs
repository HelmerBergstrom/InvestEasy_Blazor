using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace InvestEasy.Services;

// Logic for getting the current authenticated users id.
public class CurrentUserService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public CurrentUserService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    // Gets userId via AuthenticationState, which "Provides information about the currently authenticated user, if any."
    public async Task<string?> GetUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();

        // returs user information. ClaimsTypes.NameIdentifier contains users unique id in ASP.NET Identity.
        return authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
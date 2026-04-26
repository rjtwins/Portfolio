using BCT.Application.EventManagement.Events;
using BCT.Application.EventManagement.Notifiers;
using BCT.Application.UseCases.Queries;
using BCT.Blazor.State;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BCT.Blazor.Services;

internal class AuthService : IAuthService
{
    CurrentUserState _currentUserState;
    UserAuthenticatedNotifier _userAuthenticatedNotifier;
    IGetUserUseCase _getUserUseCase;

    public AuthService(CurrentUserState currentUserState, UserAuthenticatedNotifier userAuthenticatedNotifier, IGetUserUseCase getUserUseCase)
    {
        _currentUserState = currentUserState;
        _userAuthenticatedNotifier = userAuthenticatedNotifier;
        _getUserUseCase = getUserUseCase;
    }

    public async Task<bool> HandleAuth(Task<AuthenticationState>? authStateTask)
    {
        _currentUserState.Value = null;

        if (authStateTask == null)
            return false;

        var authState = await authStateTask;
        var principal = authState?.User;

        if (!IsAuthenticated(principal))
            return false;

        var authId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (authId == null)
            return false;

        _userAuthenticatedNotifier.Notify(new UserAuthenticatedEvent(authId, true));

        _currentUserState.Value = await _getUserUseCase.ExecuteAsync(authId);

        return true;
    }

    private bool IsAuthenticated(ClaimsPrincipal? principal)
    {
        return (principal?.Identity?.IsAuthenticated ?? false) == true;
    }
}
using Microsoft.AspNetCore.Components.Authorization;

namespace BCT.Blazor.Services;

internal interface IAuthService
{
    Task<bool> HandleAuth(Task<AuthenticationState>? authStateTask);
}
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BCT.Blazor.Services;

internal class MyCircuitHandler : CircuitHandler
{
    private readonly AuthenticationStateProvider authenticationStateProvider;
    private readonly SessionTracker sessionTracker;

    public MyCircuitHandler(AuthenticationStateProvider authenticationStateProvider, SessionTracker session)
    {
        this.authenticationStateProvider = authenticationStateProvider;
        this.sessionTracker = session;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var userId = await GetUserId();
        sessionTracker.UserStartedSession(userId);

        await base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var userId = await GetUserId();
        sessionTracker.UserStoppedSession(userId);

        await base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    private async Task<string?> GetUserId()
    {
        var auth = await authenticationStateProvider.GetAuthenticationStateAsync();
        return auth?.User?.Identity?.Name;
    }
}
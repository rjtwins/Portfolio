using Microsoft.Extensions.Logging;
using BCT.Application.ServiceInterfaces;
using BCT.Application.AuthEntities;

namespace BCT.Auth0Api;

public class Auth0ManagementTokenRefreshService : IAuth0ManagementTokenRefreshService
{
	private readonly ILogger<Auth0ManagementTokenRefreshService> _logger;
	private readonly IAuthManagementService _auth0ManagementApi;
	private readonly AuthToken _auth0Token;

	public Auth0ManagementTokenRefreshService(
		ILogger<Auth0ManagementTokenRefreshService> logger,
		IAuthManagementService auth0ManagementApi,
		AuthToken auth0Token)
	{
		_logger = logger;
		_auth0ManagementApi = auth0ManagementApi;
		_auth0Token = auth0Token;
	}

    public async Task CheckAndRefresh()
    {
        if (!_auth0Token.NearlyExpired)
            return;

        _logger.LogInformation("Auth0 token is nearly expired. Refreshing token...");

        try
        {
            var freshToken = await _auth0ManagementApi.GetAuth0ManagementApiToken();
            _auth0Token.Refresh(freshToken);

            _logger.LogInformation("Auth0 token refreshed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing Auth0 token.");
        }
    }
}
namespace BCT.Application.UseCases.Commands;

public class RemoveRoleFromUserUseCase : IRemoveRoleFromUserUseCase
{
	private readonly IAuthManagementService auth0ManagementApi;
	public RemoveRoleFromUserUseCase(IAuthManagementService auth0ManagementApi)
	{
		this.auth0ManagementApi = auth0ManagementApi;
	}
	
	public async Task ExecuteAsync(User user, Role role)
	{
		if(user == null)
			throw new ArgumentNullException(nameof(user));

        if (role == null)
            throw new ArgumentNullException(nameof(role));

        await ExecuteAsync(user.AuthId, role.Auth0Id);
	}

	public async Task ExecuteAsync(string userAuthId, string roleAuthId)
	{
		if(string.IsNullOrWhiteSpace(userAuthId))
			throw new ArgumentNullException(nameof(userAuthId));
			
		if(string.IsNullOrEmpty(roleAuthId))
			throw new ArgumentNullException(nameof(roleAuthId));
		
		await auth0ManagementApi.RemoveUserRole(userAuthId, roleAuthId);
	}
}

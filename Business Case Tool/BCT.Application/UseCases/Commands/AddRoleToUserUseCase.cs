namespace BCT.Application.UseCases.Commands;

public class AddRoleToUserUseCase : IAddRoleToUserUseCase
{
	private readonly IAuthManagementService auth0ManagementApi;
	public AddRoleToUserUseCase(IAuthManagementService auth0ManagementApi)
	{
		this.auth0ManagementApi = auth0ManagementApi;
	}
	
	public Task ExecuteAsync(User user, Role role)
	{	
		if(user == null)
			throw new ArgumentNullException(nameof(user));
		if(role == null)
			throw new ArgumentNullException(nameof(role));
			
		return ExecuteAsync(user.AuthId, role.Auth0Id);
	}

	public async Task ExecuteAsync(string userAuthId, string roleAuthId)
	{
		if(string.IsNullOrEmpty(userAuthId))
			throw new ArgumentNullException(nameof(userAuthId));
		if(string.IsNullOrEmpty(roleAuthId))
			throw new ArgumentNullException(nameof(roleAuthId));
		
		await auth0ManagementApi.AddUserRole(userAuthId, roleAuthId);
	}
}

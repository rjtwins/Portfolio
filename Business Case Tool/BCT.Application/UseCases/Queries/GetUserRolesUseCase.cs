namespace BCT.Application.UseCases.Queries;

public class GetUserRolesUseCase : IGetUserRolesUseCase
{
	private readonly IAuthManagementService auth0ManagementApi;
	private readonly IRepository<User> userRepository;

	public GetUserRolesUseCase(IAuthManagementService auth0ManagementApi, IRepository<User> userRepository)
	{
		this.auth0ManagementApi = auth0ManagementApi;
		this.userRepository = userRepository;
	}
	
	public async Task<Role[]> ExecuteAsync(string auth0Id)
	{
		AuthRole[] auth0Roles;
		try
		{
			auth0Roles = await auth0ManagementApi.GetUserRoles(auth0Id);
		}
		catch (BCT.Application.Exceptions.AuthServiceToManyRequestsException)
		{
			await Task.Delay(5000);
			auth0Roles = await auth0ManagementApi.GetUserRoles(auth0Id);
		}
		
		return auth0Roles.Select(x => new Role { Name = x.name, Auth0Id = x.id, Description = x.description }).ToArray();
	}
	
	public async Task<Role[]> ExecuteAsync(int userId)
	{
		var user = await userRepository.Get(userId);
		
		if(user == null)
			throw new Exception("User not found in repository");
			
		return await ExecuteAsync(user);
	}
	
	public async Task<Role[]> ExecuteAsync(User user)
	{
		return await ExecuteAsync(user.AuthId);
	}
}

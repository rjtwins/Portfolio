namespace BCT.Application.UseCases.Queries;

public sealed class GetUsersUseCase : IGetUsersUseCase
{
	private readonly IRepository<User> _userRepository;
	private readonly IAuthManagementService _auth0ManagementApi;
	private readonly IGetUserRolesUseCase _getUserRolesUseCase;

	public GetUsersUseCase(IRepository<User> userRepository, IAuthManagementService auth0ManagementApi, IGetUserRolesUseCase getUserRolesUseCase)
	{
		_userRepository = userRepository;
		_auth0ManagementApi = auth0ManagementApi;
		_getUserRolesUseCase = getUserRolesUseCase;
	}
	
	public async Task<User[]> ExecuteAsync()
	{
		var auth0users = await _auth0ManagementApi.GetAllUsers();
		var auth0Ids = auth0users.Select(x => x.user_id).ToList();
		var dbUsers = (await _userRepository.GetAll(x => auth0Ids.Contains(x.AuthId))).ToList();
		
		foreach(var dbUser in dbUsers)
		{
			var auth0User = auth0users.First(x => x.user_id == dbUser.AuthId);
			dbUser.UpdateFrom(auth0User);
			dbUser.Roles = (await _getUserRolesUseCase.ExecuteAsync(auth0User.user_id)).ToList();
		}
				
		return dbUsers.ToArray();
	}
}

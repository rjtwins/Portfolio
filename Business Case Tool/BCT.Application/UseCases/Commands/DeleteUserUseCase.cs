namespace BCT.Application.UseCases.Commands;
public class DeleteUserUseCase : IDeleteUserUseCase
{
	private readonly IAuthManagementService auth0ManagementApi;
	private readonly IRepository<User> userRepository;

	public DeleteUserUseCase(IAuthManagementService auth0ManagementApi, IRepository<User> userRepository)
	{
		this.auth0ManagementApi = auth0ManagementApi;
		this.userRepository = userRepository;
	}

	public async Task ExecuteAsync(User user)
	{
		if(user == null)
		{
			throw new ArgumentNullException(nameof(user));
		}
		
		await auth0ManagementApi.DeleteUser(user.AuthId);
		await userRepository.Delete(user);
	}
}

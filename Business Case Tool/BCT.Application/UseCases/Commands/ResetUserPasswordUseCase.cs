namespace BCT.Application.UseCases.Commands;
public class ResetUserPasswordUseCase : IResetUserPasswordUseCase
{
	private readonly IAuthManagementService auth0ManagementApi;

	public ResetUserPasswordUseCase(IAuthManagementService auth0ManagementApi) 
	{
		this.auth0ManagementApi = auth0ManagementApi;
	}

	public async Task ExecuteAsync(User user)
	{
		if(user == null)
			throw new ArgumentNullException(nameof(user));
			
		if(string.IsNullOrWhiteSpace(user.Email))
			throw new ArgumentNullException(nameof(user.Email));
			
		await auth0ManagementApi.ResetPasswordByEmail(user.Email);
	}
}

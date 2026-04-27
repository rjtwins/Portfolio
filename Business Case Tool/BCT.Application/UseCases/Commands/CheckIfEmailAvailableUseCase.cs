namespace BCT.Application.UseCases.Commands;

public class CheckIfEmailAvailableUseCase : ICheckIfEmailAvailableUseCase
{
	private readonly IAuthManagementService authManagementService;

	public CheckIfEmailAvailableUseCase(IAuthManagementService authManagementService) 
	{
		this.authManagementService = authManagementService;
	}

	public async Task<bool> ExecuteAsync(string email)
	{
		if(string.IsNullOrEmpty(email))
			throw new ArgumentNullException(nameof(email));
			
		var allUsers = await authManagementService.GetAllUsers();
		
		return !allUsers.Any(u => u.email == email);
	}
}


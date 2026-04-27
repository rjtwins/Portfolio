namespace BCT.Application.UseCases.Queries;

public class GetAllRolesUseCase : IGetAllRolesUseCase
{
	private readonly IAuthManagementService auth0ManagementApi;

	public GetAllRolesUseCase(IAuthManagementService auth0ManagementApi)
	{
		this.auth0ManagementApi = auth0ManagementApi;
	}
	
	public async Task<Role[]> ExecuteAsync()
	{
		AuthRole[] authRoles;
        authRoles = await auth0ManagementApi.GetAllRoles();
        return authRoles.Select(r => r.ToDomainEntity()).ToArray();
	}
}

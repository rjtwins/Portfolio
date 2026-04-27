namespace BCT.Application.UseCases.Queries;
public class CheckIfUserExistsUseCase : ICheckIfUserExistsUseCase
{
    private readonly IAuthManagementService authManagementService;
    private readonly IRepository<User> userRepository;
    private readonly IGetUserRolesUseCase getUserRolesUseCase;

    public CheckIfUserExistsUseCase(IAuthManagementService authManagementService, IRepository<User> userRepository, IGetUserRolesUseCase getUserRolesUseCase)
    {
        this.authManagementService = authManagementService;
        this.userRepository = userRepository;
    }


    public async Task<bool> ExecuteAsync(string authId)
    {
        if (string.IsNullOrWhiteSpace(authId))
            throw new ArgumentNullException(nameof(authId));

        try
        {
            var authUser = await authManagementService.GetUserByAuthId(authId);
            if (authUser == null)
                return false;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

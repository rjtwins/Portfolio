namespace BCT.Application.UseCases.Queries;
public class GetUserUseCase : IGetUserUseCase
{
    private readonly IAuthManagementService authManagementService;
    private readonly IRepository<User> userRepository;
    private readonly IGetUserRolesUseCase getUserRolesUseCase;

    public GetUserUseCase(IAuthManagementService authManagementService, IRepository<User> userRepository, IGetUserRolesUseCase getUserRolesUseCase)
    {
        this.authManagementService = authManagementService;
        this.userRepository = userRepository;
        this.getUserRolesUseCase = getUserRolesUseCase;
    }


    public async Task<User> ExecuteAsync(string authId)
    {
        if (string.IsNullOrWhiteSpace(authId))
            throw new ArgumentNullException(nameof(authId));

        var authUser = await authManagementService.GetUserByAuthId(authId);

        if (authUser == null)
            throw new Exception("User not found in auth provider.");

        var user = await userRepository.FirstOrDefault(x => x.AuthId == authId);

        if (user == null)
            throw new Exception("User not found in repository");

        user.UpdateFrom(authUser);
        user.Roles = (await getUserRolesUseCase.ExecuteAsync(user.AuthId)).ToList();

        return user;
    }
}

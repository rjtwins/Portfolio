namespace BCT.Application.UseCases.Commands;
public class UpdateLastSelectedUseCase : IUpdateLastSelectedUseCase
{
    private readonly IRepository<User> userRepository;

    public UpdateLastSelectedUseCase(IRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task Execute(int userId, int? projectId, int? companyId)
    {
        var user = await userRepository.Get(userId);
        if (user == null)
            throw new Exception("User not found");


        user.LastProjectId = projectId;
        user.LastCompanyId = companyId;

        await userRepository.Update(user);
    }
}

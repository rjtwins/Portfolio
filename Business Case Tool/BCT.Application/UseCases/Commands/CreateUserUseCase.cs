namespace BCT.Application.UseCases.Commands;
public class CreateUserUseCase : ICreateUserUseCase
{
    private readonly IAuthManagementService authManagementService;
    private readonly IRepository<User> userRepository;

    public CreateUserUseCase(IAuthManagementService authManagementService, IRepository<User> userRepository)
    {
        this.authManagementService = authManagementService;
        this.userRepository = userRepository;
    }

    public async Task<string> ExecuteAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));

        var result = await authManagementService.TryCreateUser(email);

        if (!result.result)
        {
            await FixUserAsync(email);
        }

        var createdUserId = result.userId;

        await authManagementService.ResetPasswordByEmail(email);

        if (string.IsNullOrEmpty(createdUserId))
            return string.Empty;

        var newLocalUser = new User
        {
            Name = email,
            AuthId = createdUserId,
            Email = email,
        };

        await userRepository.Add(newLocalUser);

        return createdUserId;
    }

    /// <summary>
    /// Ensures a failed user creation is resolved by locating an existing Auth user with the specified email
    /// and deleting that user from the Auth management store.
    /// If no matching Auth user is found, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="email">The email address of the user to locate and delete in Auth.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous delete operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a user with the specified email cannot be found in Auth.</exception>
    private async Task FixUserAsync(string email) 
    {
        var users = await authManagementService.GetAllUsers();
        var existingUser = users.FirstOrDefault(u => u.email?.Equals(email, StringComparison.OrdinalIgnoreCase) ?? false);

        if (existingUser == null)
        {
            throw new InvalidOperationException($"User with email {email} was already in the auth database when trying to create, however we could not find it.");
        }

        await authManagementService.DeleteUser(existingUser.user_id);
    }
}

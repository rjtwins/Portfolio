using BCT.Application.AuthEntities;

namespace BCT.Application.ServiceInterfaces;

public interface IAuthManagementService
{
    Task AddUserRole(string token, string userId);
    Task<(bool result, string userId)> TryCreateUser(string userEmail);
    Task DeleteUser(string userId);
    Task<AuthRole[]> GetAllRoles();
    Task<AuthUser[]> GetAllUsers();
    Task<AuthUser> GetUserByAuthId(string auth0Id);
    Task<AuthRole[]> GetUserRoles(string userId);
    Task<AuthToken> GetAuth0ManagementApiToken();
    Task RemoveUserRole(string userId, string roleId);
    Task ResetPasswordByEmail(string userEmail);
}

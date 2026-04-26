using BCT.Domain.Entities;

namespace BCT.Application.ServiceInterfaces;
public interface ICompanyRepository : IRepository<Company>
{
    Task AddUserToCompany(User user, Company company);
    Task<List<User>> GetCompanyUsers(Company company);
    Task RemoveUserFromCompany(User user, Company company);
    Task<List<Tag>> GetCompanyTags(int companyId);
    Task<Company?> GetTracked(int companyId);

    Task<Tag> AddTagToCompany(string tag, int companyId, int? projectId);
}
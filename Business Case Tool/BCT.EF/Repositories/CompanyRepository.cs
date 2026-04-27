using BCT.Application.ServiceInterfaces;
using BCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BCT.EF.Repositories;
public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {

    }

    public async Task<Tag> AddTagToCompany(string text, int companyId, int? projectId)
    {
        using var context = GetContext();

        var company = await context.Companies
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == companyId);

        var project = await context.Projects
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (company == null)
            throw new InvalidOperationException($"Could not find company with id {companyId}");

        var tag = new Tag()
        {
            Projects = project == null ? new List<Project>() : new List<Project>() { project },
            Company = company,
            CompanyId = company.Id,
            Text = text
        };

        tag = (await context.Tags.AddAsync(tag)).Entity;

        context.SaveChanges();

        return await context.Tags.AsNoTracking().FirstAsync(x => x.Id == tag.Id);
    }

    public async Task<Company?> GetTracked(int companyId)
    {
        using var context = GetContext();
        return await context.Companies
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == companyId);
    }

    public async Task AddUserToCompany(User user, Company company)
    {
        using var context = GetContext();

        var dbCompany = context.Companies
            .AsTracking()
            .FirstOrDefault(x => x.Id == company.Id);

        var dbUser = context
            .Users
            .AsTracking()
            .Include(x => x.Companies)
            .FirstOrDefault(x => x.Id == user.Id);

        if (dbCompany == null || dbUser == null)
        {
            throw new Exception("Company or User not found");
        }

        dbUser.Companies.Add(dbCompany);

        await context.SaveChangesAsync();
    }

    public async Task RemoveUserFromCompany(User user, Company company)
    {
        using var context = GetContext();

        var dbCompany = context.Companies
            .AsTracking()
            .FirstOrDefault(x => x.Id == company.Id);

        var dbUser = context
            .Users
            .AsTracking()
            .Include(x => x.Companies)
            .FirstOrDefault(x => x.Id == user.Id);

        if (dbCompany == null || dbUser == null)
        {
            throw new Exception("Company or User not found");
        }

        dbUser.Companies.Remove(dbCompany);
        await context.SaveChangesAsync();
    }

    public async Task<List<User>> GetCompanyUsers(Company c)
    {
        using var context = GetContext();

        return (await context.Companies.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == c.Id))?.Users.ToList() ?? new();
    }

    public async Task<List<Tag>> GetCompanyTags(int companyId)
    {
        using var context = GetContext();

        var company = await context.Companies
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        return company?.Tags ?? new();
    }
}

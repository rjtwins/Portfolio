using BCT.Application.ServiceInterfaces;
using BCT.Application.Services;

namespace BCT.Application.UseCases.Commands;

public class CreateCompanyUseCase : ICreateCompanyUseCase
{
    private readonly ICompanyRepository companyRepository;
    private readonly NewCompanyNotifier newCompanyNotifier;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public CreateCompanyUseCase(
        ICompanyRepository companyRepository, 
        NewCompanyNotifier newCompanyNotifier)
    {
        this.companyRepository = companyRepository;
        this.newCompanyNotifier = newCompanyNotifier;
    }

    public async Task<Company> ExecuteAsync(User creator, string companyName, string userId)
    {
        //We should only make one company at a time!
        await _lock.WaitAsync();
        try
        {
            var company = new Company()
            {
                Name = companyName,
                CreatorId = creator.Id
            };

            company = await companyRepository.Add(company);
            await companyRepository.AddUserToCompany(creator, company);

            Domain.Configuration.Project.GlobalTags
                .ToList()
                .ForEach(x =>
                {
                    companyRepository.AddTagToCompany(x, company.Id, null);
                });

            newCompanyNotifier.Notify(new NewCompanyEvent(company.Id, userId));

            return company;
        }
        finally
        {
            _lock.Release();

        }
    }
}

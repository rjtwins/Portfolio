using BCT.Application.Services;

namespace BCT.Application.UseCases.Commands;

public class SaveCompanyUseCase : ISaveCompanyUseCase
{
    private readonly ICompanyRepository companyRepository;
    private readonly CompanyContentUpdatedNotifier companyContentUpdatedNotifier;

    public SaveCompanyUseCase(ICompanyRepository companyRepository,
        CompanyContentUpdatedNotifier companyContentUpdatedNotifier)
    {
        this.companyRepository = companyRepository;
        this.companyContentUpdatedNotifier = companyContentUpdatedNotifier;
    }

    public async Task<Company> ExecuteAsync(Company company, string userId)
    {
        var updated = await companyRepository.Update(company);

        companyContentUpdatedNotifier.Notify(new(company, userId));

        return updated;
    }
}

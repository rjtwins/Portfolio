using BCT.Application.Services;

namespace BCT.Application.UseCases.Commands;

public class DeleteCompanyUseCase : IDeleteCompanyUseCase
{
    private readonly ICompanyRepository companyRepository;
    private readonly CompanyRemovedNotifier companyRemovedNotifier;

    public DeleteCompanyUseCase(ICompanyRepository companyRepository, CompanyRemovedNotifier companyRemovedNotifier)
    {
        this.companyRepository = companyRepository;
        this.companyRemovedNotifier = companyRemovedNotifier;
    }

    public async Task ExecuteAsync(Company company, string userId)
    {
        await companyRepository.Delete(company);

        companyRemovedNotifier.Notify(new(company.Id, userId));
    }
}
using System.Linq;

namespace BCT.Application.UseCases.Commands;
public class ProcessMissingValueMigrationsUseCase : IProcessMissingValueMigrationsUseCase
{
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly IRepository<StringValue> stringValueRepository;
    private readonly IRepository<BoolValue> boolValueRepository;
    private readonly IRepository<Project> projectRepository;
    private readonly ICompanyRepository companyRepository;
    private readonly ITagRepository tagRepository;

    public ProcessMissingValueMigrationsUseCase(
        IRepository<DoubleValue> doubleValueRepository,
        IRepository<StringValue> stringValueRepository,
        IRepository<BoolValue> boolValueRepository,
        IRepository<Project> projectRepository,
        ICompanyRepository companyRepository,
        ITagRepository tagRepository
        )
    {
        this.doubleValueRepository = doubleValueRepository;
        this.stringValueRepository = stringValueRepository;
        this.boolValueRepository = boolValueRepository;
        this.projectRepository = projectRepository;
        this.companyRepository = companyRepository;
        this.tagRepository = tagRepository;
    }


    public async Task Execute()
    {
        //Projects
        var projects = await projectRepository.GetAll();
        foreach (Project project in projects)
        {
            await FixMissingProjectValues(project);
        }

        var companies = await companyRepository.GetAll();
        foreach (Company company in companies)
        {
            await FixMissingCompanyValues(company);
        }


        SetVerdienmodelEnabled();
    }

    private async void SetVerdienmodelEnabled()
    {
        var toUpdate = await boolValueRepository.GetAll(x => x.Key == "VerdienmodelEnabled" && x.Value == false);
        toUpdate.ForEach(x => 
        {
            x.Value = true;
            boolValueRepository.Update(x);
        });
    }

    private async Task FixMissingCompanyValues(Company company)
    {
        var tags = await companyRepository.GetCompanyTags(company.Id);
        var allTagsPresent = Configuration.Project.GlobalTags.All(x => tags.Select(y => y.Text).Contains(x));

        if (allTagsPresent)
            return;

        var missingTags = Configuration.Project.GlobalTags.Where(x => !tags.Select(y => y.Text).Contains(x)).ToList();
        var newTags = missingTags.Select(x => new Tag
        {
            Text = x,
            CompanyId = company.Id,
            Company = company
        }).ToList();

        company.Tags = newTags;
        await companyRepository.Update(company);
    }

    private async Task FixMissingProjectValues(Project project)
    {
        var startYear = project.StartYear;
        var horizon = project.Horizon;

        var attributes = Configuration.Project.Attributes
            .Where(x => !x.Calculated)
            .ToList();

        var overTimeAttributes = Configuration.Project.OverTimeAttributes
            .Where(x => !x.Calculated)
            .ToList();

        var doubleValues = new List<DoubleValue>();
        var stringValues = new List<StringValue>();
        var boolValues = new List<BoolValue>();

        List<ValueModel> allValues = new();
        allValues.AddRange(await doubleValueRepository.GetAll(x => x.ProjectId == project.Id));
        allValues.AddRange(await stringValueRepository.GetAll(x => x.ProjectId == project.Id));
        allValues.AddRange(await boolValueRepository.GetAll(x => x.ProjectId == project.Id));

        //Over time
        for (int i = startYear; i < startYear + horizon; i++)
        {
            foreach (var a in overTimeAttributes)
            {
                if (allValues.Any(x => x.Year == i && x.Key == a.Key))
                    continue;

                var doubleValue = new DoubleValue()
                {
                    ProjectId = project.Id,
                    Key = a.Key,
                    Year = i,
                    Value = 0
                };
                doubleValues.Add(doubleValue);
            }
        }

        //Project
        foreach (var a in attributes)
        {
            if (allValues.Any(x => x.Key == a.Key))
                continue;

            switch (a.AttributeType)
            {
                case Const.AttributeType.String:
                    var stringValue = new StringValue()
                    {
                        ProjectId = project.Id,
                        Key = a.Key,
                        Value = string.Empty
                    };
                    stringValues.Add(stringValue);
                    break;
                case Const.AttributeType.Double:
                    var doubleValue = new DoubleValue()
                    {
                        ProjectId = project.Id,
                        Key = a.Key,
                        Value = 0
                    };
                    doubleValues.Add(doubleValue);
                    break;
                case Const.AttributeType.Bool:
                    var boolValue = new BoolValue()
                    {
                        ProjectId = project.Id,
                        Key = a.Key,
                        Value = false
                    };

                    //This should be default on.
                    //TODO: set this in domain and not in application layer.
                    if (a.Key == "VerdienmodelEnabled")
                    {
                        boolValue.Value = true;
                    }

                    boolValues.Add(boolValue);
                    break;
                default:
                    break;
            }
        }

        await doubleValueRepository.AddRange(doubleValues);
        await stringValueRepository.AddRange(stringValues);
        await boolValueRepository.AddRange(boolValues);
    }
}

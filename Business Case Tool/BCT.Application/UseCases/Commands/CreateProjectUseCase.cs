using BCT.Application.Services;

namespace BCT.Application.UseCases.Commands;
public class CreateProjectUseCase : ICreateProjectUseCase
{
    private readonly IRepository<Project> projectRepository;
    private readonly NewProjectNotifier newProjectNotifier;
    //private readonly IRepository<Domain.Entities.Attribute> attributeRepository;
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly IRepository<StringValue> stringValueRepository;
    private readonly IRepository<BoolValue> boolValueRepository;

    public CreateProjectUseCase(
        IRepository<Project> projectRepository, 
        NewProjectNotifier newProjectNotifier,
        IRepository<DoubleValue> doubleValueRepository,
        IRepository<StringValue> stringValueRepository,
        IRepository<BoolValue> boolValueRepository)
    {
        this.projectRepository = projectRepository;
        this.newProjectNotifier = newProjectNotifier;
        this.doubleValueRepository = doubleValueRepository;
        this.stringValueRepository = stringValueRepository;
        this.boolValueRepository = boolValueRepository;
    }

    public async Task<Project> ExecuteAsync(Company company, string userId, string projectName = "")
    {
        if (string.IsNullOrEmpty(projectName))
        {
            var currentProjects = (await projectRepository.GetAll(x => x.CompanyId == company.Id)).Count();
            projectName = $"Project {currentProjects + 1}";
        }

        var project = new Project()
        {
            Name = projectName,
            CompanyId = company.Id,
        };

        project = await projectRepository.Add(project);

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

        //Over time
        for (int i = startYear; i < startYear + horizon; i++)
        {
            foreach(var a in overTimeAttributes)
            {
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

        newProjectNotifier.Notify(new(project.Id, userId));

        return project;
    }
}

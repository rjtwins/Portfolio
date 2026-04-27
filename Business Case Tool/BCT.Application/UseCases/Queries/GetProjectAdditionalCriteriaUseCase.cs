namespace BCT.Application.UseCases.Queries;
public class GetProjectAdditionalCriteriaUseCase : IGetProjectAdditionalCriteriaUseCase
{
    private readonly IRepository<BoolValue> boolValueRepository;
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly IRepository<StringValue> stringValueRepository;
    private readonly ICalculation calculation;
    private readonly ICacheRegistry cacheRegistry;

    public GetProjectAdditionalCriteriaUseCase(
        IRepository<BoolValue> boolValueRepository, 
        IRepository<DoubleValue> doubleValueRepository, 
        IRepository<StringValue> stringValueRepository, 
        ICalculation calculation,
        ICacheRegistry cacheRegistry)
    {
        this.boolValueRepository = boolValueRepository;
        this.doubleValueRepository = doubleValueRepository;
        this.stringValueRepository = stringValueRepository;
        this.calculation = calculation;
        this.cacheRegistry = cacheRegistry;
    }

    public async Task<List<StringValue>> Execute(Project p)
    {
        if (cacheRegistry.TryGet($"{p.Id}_GetProjectAdditionalCriteria_Calculated", out object cacheValue))
        {
            if (!(cacheValue is List<StringValue> unpacked))
                return new();

            return unpacked;
        }

        var stringKeys = Configuration.Project.AdditionalCriteriaOptions.ToList();
        var doubleKeys = stringKeys.Select(x => x + "Weight").ToList();
        var bookKeys = stringKeys.Select(x => x + "Enabled").ToList();

        var stringValues = await stringValueRepository.GetAll(x => x.ProjectId == p.Id && stringKeys.Contains(x.Key));
        var doubleValues = await doubleValueRepository.GetAll(x => x.ProjectId == p.Id && doubleKeys.Contains(x.Key));
        var boolValues = await boolValueRepository.GetAll(x => x.ProjectId == p.Id && bookKeys.Contains(x.Key));

        return calculation.GetAdditionalCriteria(boolValues, doubleValues, stringValues);
    }
}

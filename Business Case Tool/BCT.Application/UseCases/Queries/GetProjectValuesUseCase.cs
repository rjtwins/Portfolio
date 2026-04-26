namespace BCT.Application.UseCases.Queries;
public class GetProjectValuesUseCase : IGetProjectValuesUseCase
{
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly IRepository<StringValue> stringValueRepository;
    private readonly IRepository<BoolValue> boolValueRepository;
    private readonly ICacheRegistry cacheRegistry;

    public GetProjectValuesUseCase(
        IRepository<DoubleValue> doubleValueRepository, 
        IRepository<StringValue> stringValueRepository, 
        IRepository<BoolValue> boolValueRepository,
        ICacheRegistry cacheRegistry)
    {
        this.doubleValueRepository = doubleValueRepository;
        this.stringValueRepository = stringValueRepository;
        this.boolValueRepository = boolValueRepository;
        this.cacheRegistry = cacheRegistry;
    }

    public async Task<List<ValueModel>> Execute(int projectId)
    {
        if (cacheRegistry.TryGet($"{projectId}_GetProjectValues_Combined", out object cacheValue))
        {
            if (!(cacheValue is List<ValueModel> unpacked))
                return new();

            return unpacked;
        }

        var values = new List<ValueModel>();

        var doubleValues = await doubleValueRepository.GetAll(v => v.ProjectId == projectId);
        var stringValues = await stringValueRepository.GetAll(v => v.ProjectId == projectId);
        var boolValues = await boolValueRepository.GetAll(v => v.ProjectId == projectId);

        values.AddRange(doubleValues);
        values.AddRange(stringValues);
        values.AddRange(boolValues);

        values = values.Where(x => x.Year == null).ToList();

        return values;
    }
}

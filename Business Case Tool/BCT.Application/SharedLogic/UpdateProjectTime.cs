namespace BCT.Application.SharedLogic;
public class UpdateProjectTime : IUpdateProjectTime
{
    private IRepository<DoubleValue> doubleValueRepository;
    private readonly ICacheRegistry cacheRegistry;

    public UpdateProjectTime(IRepository<DoubleValue> doubleValueRepository, ICacheRegistry cacheRegistry)
    {
        this.doubleValueRepository = doubleValueRepository;
        this.cacheRegistry = cacheRegistry;
    }

    public async Task ExecuteAsync(Project p)
    {
        var attributes = Configuration.Project.OverTimeAttributes
                    .Where(x => !x.Calculated)
                    .ToList();

        var overTimeValues = await doubleValueRepository
            .GetAll(x => x.ProjectId == p.Id && attributes
                .Select(y => y.Key)
                .Contains(x.Key));

        var newValues = new List<DoubleValue>();

        for (int y = p.StartYear; y < p.StartYear + p.Horizon; y++)
        {
            var yearValues = overTimeValues.Where(x => x.Year == y).ToList();
            foreach (var a in attributes)
            {
                if (yearValues.Any(x => x.Key == a.Key))
                    continue;

                var value = new DoubleValue()
                {
                    Key = a.Key,
                    ProjectId = p.Id,
                    Year = y,
                    Value = 0
                };

                newValues.Add(value);
            }
        }

        if (newValues.Count > 0)
            await doubleValueRepository.AddRange(newValues);

        cacheRegistry.Remove($"{p.Id}_GetProjectDoubleValues_Combined");
    }
}

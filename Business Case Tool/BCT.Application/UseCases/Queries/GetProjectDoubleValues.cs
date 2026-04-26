using System.Collections.Generic;

namespace BCT.Application.UseCases.Queries;
public class GetProjectDoubleValues : IGetProjectDoubleValues
{
    private readonly IRepository<Project> projectRepository;
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly ICalculation calculation;
    private readonly ICacheRegistry cacheRegistry;

    public GetProjectDoubleValues(
        IRepository<Project> projectRepository,
        IRepository<DoubleValue> doubleValueRepository, 
        ICalculation calculation,
        ICacheRegistry cacheRegistry
        )
    {
        this.projectRepository = projectRepository;
        this.doubleValueRepository = doubleValueRepository;
        this.calculation = calculation;
        this.cacheRegistry = cacheRegistry;
    }

    public async Task<List<DoubleValue>> ExecuteAsync(Project project)
    {
        project = await projectRepository.Get(project?.Id ?? -1);
        
        if (project == null)
            return new();

        if (cacheRegistry.TryGet($"{project.Id}_GetProjectDoubleValues_Combined", out object cacheValue))
        {
            if (cacheValue is List<DoubleValue> unpacked)
                return unpacked;

            cacheRegistry.Remove($"{project.Id}_GetProjectDoubleValues_Combined");
        }

        var stored = (await doubleValueRepository.GetAll(x => x.ProjectId == project.Id))
            .Where(x => x.Year >= project.StartYear && x.Year < project.StartYear + project.Horizon || x.Year == null)
            .OrderBy(x => x.Key)
            .ToList();

        var calculated = calculation.CalculateProjectValues(project, stored, project.StartYear, project.Horizon);
        var combined = stored.Concat(calculated).ToList();

        cacheRegistry.Add($"{project.Id}_GetProjectDoubleValues_Combined", combined, 60 * 60 * 24 * 1000); // 1 day in ms

        return combined;
    }


    //For debug
    //private async Task<List<DoubleValue>> CalculateDoubleValues(Project project)
    //{
    //    //TODO: Implement calculation logic in calculation engine.

    //    var calculatedAttributes = Configuration.Project.OverTimeAttributes.Where(x => x.Calculated).ToList();
    //    var horizon = project.Horizon;
    //    var startYear = project.StartYear;
    //    var calculatedValues = new List<DoubleValue>();

    //    foreach (var a in calculatedAttributes)
    //    {
    //        for (int y = startYear; y < startYear + horizon; y++)
    //        {
    //            calculatedValues.Add(new() { ProjectId = project.Id, Key = a.Key, Year = y, Value = 0 });
    //        }
    //    }

    //    return calculatedValues;
    //}
}

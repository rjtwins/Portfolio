using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCT.Application.UseCases.Queries;
public class GetSensitivityAnalysisResultUseCase : IGetSensitivityAnalysisResultUseCase
{
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly ICalculation calculation;
    private readonly IRepository<Project> projectRepository;

    public GetSensitivityAnalysisResultUseCase(
        IRepository<DoubleValue> doubleValueRepository,
        ICalculation calculation,
        IRepository<Project> projectRepository)
    {
        this.doubleValueRepository = doubleValueRepository;
        this.calculation = calculation;
        this.projectRepository = projectRepository;
    }

    public async Task<List<(string Property, double PosEffect, double NegEffect)>> ExecuteAsync(int projectId, string property)
    {
        if (!Configuration.Project.SensitivityAttributesOptions.Any(x => x == property))
            throw new InvalidOperationException($"Property {property} is not a valid attribute.");

        var attribute = Domain.Configuration.Project.Attributes.Single(x => x.Key == property);

        if (!attribute.Calculated)
            throw new InvalidOperationException($"Property {property} is not a calculated attribute.");

        var project = await projectRepository.Get(projectId);

        if (project == null)
            throw new InvalidOperationException($"Project with ID {projectId} not found.");

        var baseStored = (await doubleValueRepository.GetAll(x => x.ProjectId == projectId))
                .Where(x => x.Year >= project.StartYear && x.Year < project.StartYear + project.Horizon || x.Year == null)
                .OrderBy(x => x.Key)
                .ToList();

        var baseCalculated = calculation.CalculateProjectValues(project, baseStored, project.StartYear, project.Horizon);
        var baseValue = baseCalculated.Single(x => x.Key == property);

        var results = new List<(string Property, double PosEffect, double NegEffect)>();

        foreach (var variableAttribute in Configuration.Project.SensitivityAttributes)
        {
            var posEffect = await GetEffectedValue(project, variableAttribute, property, 1.1);
            var negEffect = await GetEffectedValue(project, variableAttribute, property, 0.9);
            var posDif = Math.Abs(posEffect - baseValue.Value) / Math.Abs(baseValue.Value);
            var negDif = Math.Abs(negEffect - baseValue.Value) / Math.Abs(baseValue.Value);

            posDif = double.IsNaN(posDif) ? 0 : posDif;
            negDif = double.IsNaN(negDif) ? 0 : negDif;

            results.Add((variableAttribute, posDif, negDif));
        }

        return results;
    }

    private async Task<double> GetEffectedValue(Project project, string variableAttribute, string property, double change)
    {
        //Fetch fresh set of values
        var stored = (await doubleValueRepository.GetAll(x => x.ProjectId == project.Id))
            .Where(x => x.Year >= project.StartYear && x.Year < project.StartYear + project.Horizon || x.Year == null)
            .OrderBy(x => x.Key)
            .ToList();

        if (Configuration.Project.SensitivityAttributesStartHorizon.Contains(variableAttribute))
            stored.Where(x => x.Key == variableAttribute).OrderBy(x => x.Year).First().Value *= change;

        if (Configuration.Project.SensitivityAttributesYearly.Contains(variableAttribute))
            stored.Where(x => x.Key == variableAttribute).ToList().ForEach(x => x.Value *= change);

        var calculated = calculation.CalculateProjectValues(project, stored, project.StartYear, project.Horizon);
        var value = calculated.Single(x => x.Key == property).Value;

        return value;
    }

}

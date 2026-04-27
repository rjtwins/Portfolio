namespace BCT.Application.UseCases.Queries;
public class GetScenarioResultsUseCase : IGetScenarioResultsUseCase
{
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly ICalculation calculation;

    public GetScenarioResultsUseCase(
        IRepository<DoubleValue> doubleValueRepository,
        ICalculation calculation)
    {
        this.doubleValueRepository = doubleValueRepository;
        this.calculation = calculation;
    }

    public async Task<List<DoubleValue>> ExecuteAsync(Project project, Scenario s)
    {
        var stored = (await doubleValueRepository.GetAll(x => x.ProjectId == project.Id))
            .Where(x => x.Year >= project.StartYear && x.Year < project.StartYear + project.Horizon || x.Year == null)
            .OrderBy(x => x.Key)
            .ToList();


        //Applying sceneario:
        if(s.Mode == Enums.ScnearioMode.Relative)
        {
            ApplyRelative(s, stored);
        }
        else
        {
            ApplyAbsolute(s, stored);
        }

        var calculated = calculation.CalculateProjectValues(project, stored, project.StartYear, project.Horizon);
        var combined = stored.Concat(calculated).ToList();

        return combined;
    }

    private void ApplyRelative(Scenario s, List<DoubleValue> stored)
    {
        stored.Where(x => x.Key == "Nieuwe investeringen").ToList().ForEach(x => x.Value *= (1 + s.InvestmentMod));
        stored.Where(x => x.Key == "Vermeden investeringen").ToList().ForEach(x => x.Value *= (1 + s.AvoidedInvestmentMod));

        ApplyCostMod(s.CostMod, stored);
        ApplyAvoidedCostMod(s.AvoidedCostMod, stored);

        stored.Where(x => x.Key == "Extra inkomsten").ToList().ForEach(x => x.Value *= (1 + s.IncomeMod));
        stored.Where(x => x.Key == "Gederfde inkomsten").ToList().ForEach(x => x.Value *= (1 + s.LostIncomeMod));
    }

    private void ApplyCostMod(double mod, List<DoubleValue> stored)
    {
        var costDoubleValues = stored.Where(x => Domain.Configuration.Project.ProjectGridWizardCostCatagory.Contains(x.Key)).ToList();
        var years = costDoubleValues.Select(x => x.Year).Distinct().OrderBy(x => x).ToList();

        foreach(int year in years)
        {
            var inYear = stored.Where(x => x.Year == year).ToList();
            var total = inYear.Sum(x => x.Value);
            var added = total * mod;
            inYear.Where(x => x.Key == "Overig").ToList().ForEach(x => x.Value += added);
        }
    }

    private void ApplyAvoidedCostMod(double mod, List<DoubleValue> stored)
    {
        var costDoubleValues = stored.Where(x => Domain.Configuration.Project.ProjectGridWizardAvoidedCostCatagory.Contains(x.Key)).ToList();
        var years = costDoubleValues.Select(x => x.Year).Distinct().OrderBy(x => x).ToList();

        foreach (int year in years)
        {
            var inYear = stored.Where(x => x.Year == year).ToList();
            var total = inYear.Sum(x => x.Value);
            var added = total * mod;
            inYear.Where(x => x.Key == "Overig2").ToList().ForEach(x => x.Value += added);
        }
    }

    private void ApplyAbsolute(Scenario s,  List<DoubleValue> stored)
    {
        stored.Where(x => x.Key == "Nieuwe investeringen").ToList().First().Value += s.InvestmentDif;
        stored.Where(x => x.Key == "Vermeden investeringen").ToList().First().Value += s.AvoidedInvestmentDif;

        stored
            .Where(x => x.Key == Domain.Configuration.Project.ProjectGridWizardCostCatagory.First())
            .ToList()
            .ForEach(x => x.Value += s.CostDif);

        stored
            .Where(x => x.Key == Domain.Configuration.Project.ProjectGridWizardAvoidedCostCatagory.First())
            .ToList()
            .ForEach(x => x.Value += s.AvoidedCostDif);

        stored.Where(x => x.Key == "Extra inkomsten").ToList().ForEach(x => x.Value += s.IncomeDif);
        stored.Where(x => x.Key == "Gederfde inkomsten").ToList().ForEach(x => x.Value += s.LostIncomeDif);
    }
}

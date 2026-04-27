using BCT.Application.Services;
using BCT.Application.SharedLogic;
using BCT.Domain.Entities;

namespace BCT.Application.UseCases.Commands;
public class ApplyProjectGridWizardUseCase : IApplyProjectGridWizardUseCase
{
    private readonly IRepository<Project> projectRepository;
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly ProjectContentUpdatedNotifier projectContentUpdatedNotifier;
    private readonly IUpdateProjectTime updateProjectTime;

    public ApplyProjectGridWizardUseCase(
        IRepository<Project> projectRepository,
        IRepository<DoubleValue> doubleValueRepository,
        ProjectContentUpdatedNotifier projectContentUpdatedNotifier,
        IUpdateProjectTime updateProjectTime)
    {
        this.projectRepository = projectRepository;
        this.doubleValueRepository = doubleValueRepository;
        this.projectContentUpdatedNotifier = projectContentUpdatedNotifier;
        this.updateProjectTime = updateProjectTime;
    }

    public async Task ExecuteAsync(ProjectGridWizard? projectGridWizard, string userId)
    {
        if(projectGridWizard == null)
            return;

        var project = await projectRepository.Get(projectGridWizard.ProjectId);

        if (project == null)
            return;

        if (project.StartYear != projectGridWizard.StartYear || project.Horizon != projectGridWizard.Horizon)
        {
            project.StartYear = projectGridWizard.StartYear;
            project.Horizon = projectGridWizard.Horizon;
            await updateProjectTime.ExecuteAsync(project);
            project = await projectRepository.Update(project);
        }

        project.InterestEnabled = projectGridWizard.InterestEnabled;

        project = await projectRepository.Update(project);

        var doubleValues = (await doubleValueRepository.GetAll(x => x.ProjectId == project.Id))
            .Where(x => x.Year >= project.StartYear && x.Year < project.StartYear + project.Horizon || x.Year == null)
            .OrderBy(x => x.Key)
            .ToList();

        //Reset double values that are in the grid.
        Configuration.Project.OverTimeAttributes.ToList().ForEach(x =>
        {
            doubleValues.Where(y => y.Key == x.Key).ToList().ForEach(y => y.Value = 0);
        });

        doubleValues.FirstOrDefault(x => x.Key == "Interest")!.Value = projectGridWizard.Interest;

        ApplyInvestment(projectGridWizard, doubleValues);

        ApplyCost(projectGridWizard, doubleValues);

        ApplyIncome(projectGridWizard, doubleValues);

        ApplyResidual(projectGridWizard, doubleValues);

        foreach (var doubleValue in doubleValues)
        {
            await doubleValueRepository.Update(doubleValue);
        }

        projectContentUpdatedNotifier.Notify(new(project!, userId));
    }

    private void ApplyInvestment(ProjectGridWizard projectGridWizard, List<DoubleValue> doubleValues)
    {
        //Investment:
        var value = projectGridWizard.NewInvestment;
        var startYear = projectGridWizard.NewInvestmentYear;

        List<DoubleValue> projectValues = doubleValues
            .Where(x => x.Key == "Nieuwe investeringen")
            .Where(x => x.Year == startYear)
            .OrderBy(x => x.Year)
            .ToList();

        projectValues.ForEach(x => x.Value = value);

        if (!projectGridWizard.AvoidedInvestmentEnabled)
            return;

        value = projectGridWizard.AvoidedInvestment;
        startYear = projectGridWizard.AvoidedInvestmentYear;

        projectValues = doubleValues
            .Where(x => x.Key == "Vermeden investeringen")
            .Where(x => x.Year == startYear)
            .OrderBy(x => x.Year)
            .ToList();

        projectValues.ForEach(x => x.Value = value);
    }

    private void ApplyCost(ProjectGridWizard projectGridWizard, List<DoubleValue> doubleValues)
    {
        Type type = typeof(ProjectGridWizard);

        foreach(var item in Configuration.Project.ProjectGridWizardCostCatagory)
        {
            var propertyName = Configuration.Project.OverTimeValueWizardMap[item];
            double value = (double)type.GetProperty(propertyName)!.GetValue(projectGridWizard)!;
            double year = (int)type.GetProperty(propertyName + "Year")!.GetValue(projectGridWizard)!;

            List<DoubleValue> projectValues = doubleValues
                .Where(x => x.Key == item)
                .Where(x => x.Year >= year)
                .OrderBy(x => x.Year)
                .ToList();

            projectValues.ForEach(x => x.Value = value);
        }

        if (!projectGridWizard.AvoidedCostEnabled)
            return;

        foreach (var item in Configuration.Project.ProjectGridWizardAvoidedCostCatagory)
        {
            var propertyName = Configuration.Project.OverTimeValueWizardMap[item];
            double value = (double)type.GetProperty(propertyName)!.GetValue(projectGridWizard)!;
            double year = (int)type.GetProperty(propertyName + "Year")!.GetValue(projectGridWizard)!;

            List<DoubleValue> projectValues = doubleValues
                .Where(x => x.Key == item)
                .Where(x => x.Year >= year)
                .OrderBy(x => x.Year)
                .ToList();

            projectValues.ForEach(x => x.Value = value);
        }
    }

    private void ApplyIncome(ProjectGridWizard projectGridWizard, List<DoubleValue> doubleValues)
    {
        if (projectGridWizard.ExtraIncomeEnabled)
        {
            var year = projectGridWizard.ExtraIncomeYear;
            var value = projectGridWizard.ExtraIncome;
            var key = "Extra inkomsten";

            List<DoubleValue> projectValues = doubleValues
                .Where(x => x.Key == key)
                .Where(x => x.Year >= year)
                .OrderBy(x => x.Year)
                .ToList();

            projectValues.ForEach(x => x.Value = value);
        }

        if (projectGridWizard.LostIncomeEnabled)
        {
            var year = projectGridWizard.LostIncomeYear;
            var value = projectGridWizard.LostIncome;
            var key = "Gederfde inkomsten";

            List<DoubleValue> projectValues = doubleValues
                .Where(x => x.Key == key)
                .Where(x => x.Year >= year)
                .OrderBy(x => x.Year)
                .ToList();

            projectValues.ForEach(x => x.Value = value);
        }
    }

    private void ApplyResidual(ProjectGridWizard projectGridWizard, List<DoubleValue> doubleValues)
    {
        var value = projectGridWizard.ResidualValue;
        doubleValues.Single(x => x.Key == "Nieuwe investeringen Risidual").Value = value;
    }
}

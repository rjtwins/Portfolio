using BCT.Application.UseCases.Commands;
using BCT.Application.UseCases.Queries;
using BCT.Blazor.State;
using BCT.Domain.Entities;
using ThrottleDebounce;

namespace BCT.Blazor.Services;

internal class ProjectValueService : IProjectValueService
{
    private readonly ISaveProjectUseCase saveProjectUseCase;
    private readonly ISaveValueUseCase saveValueUseCase;
    private readonly IGetProjectValuesUseCase getProjectValuesUseCase;
    private readonly IGetProjectDoubleValues getProjectDoubleValues;
    private readonly CurrentUserState currentUserState;
    private readonly TimeSpan _debounceTimeout = TimeSpan.FromMilliseconds(1000);


    public ProjectValueService(
        ISaveProjectUseCase saveProjectUseCase,
        ISaveValueUseCase saveValueUseCase, 
        IGetProjectValuesUseCase getProjectValuesUseCase,
        IGetProjectDoubleValues getProjectDoubleValues,
        CurrentUserState currentUserState)
    {
        this.saveProjectUseCase = saveProjectUseCase;
        this.saveValueUseCase = saveValueUseCase;
        this.getProjectValuesUseCase = getProjectValuesUseCase;
        this.getProjectDoubleValues = getProjectDoubleValues;
        this.currentUserState = currentUserState;
    }

    public async Task<Dictionary<string, ValueModel>> GetProjectData(Project p)
    {
        if (p == null)
            return new();

        var values = await getProjectValuesUseCase.Execute(p.Id);
        var data = values.ToDictionary(v => v.Key, v => v);

        return data;
    }

    public T GetValue<T>(string key, Dictionary<string, ValueModel> data) where T : Domain.Entities.ValueModel
    {
        return (T)data[key];
    }

    public async Task SaveValue(string key, Dictionary<string, ValueModel> data)
    {
        var value = data[key];
        await saveValueUseCase.Execute(value, currentUserState?.Value?.Name ?? "NULL");
    }

    public async Task SaveOverTimeValue(DoubleValue doubleValue, double newValue)
    {
        doubleValue.Value = newValue;
        await saveValueUseCase.Execute(doubleValue, currentUserState?.Value?.Name ?? "NULL");
    }

    public async Task<List<DoubleValue>> GetProjectDoubleValues(Project p)
    {
        return await getProjectDoubleValues.ExecuteAsync(p);
    }

    public async Task SaveProject(Project p)
    {
        await saveProjectUseCase.ExecuteAsync(p, currentUserState?.Value?.Name ?? "NULL");
    }
}
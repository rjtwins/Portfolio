using BCT.Domain.Entities;

namespace BCT.Blazor.Services;
public interface IProjectValueService
{
    Task<Dictionary<string, ValueModel>> GetProjectData(Project p);
    Task SaveValue(string key, Dictionary<string, ValueModel> data);
    Task SaveOverTimeValue(DoubleValue doubleValue, double newValue);
    Task SaveProject(Project p);

    T GetValue<T>(string key, Dictionary<string, Domain.Entities.ValueModel> data) where T : Domain.Entities.ValueModel;
    Task<List<DoubleValue>> GetProjectDoubleValues(Project p);
}
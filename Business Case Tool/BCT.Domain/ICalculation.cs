using BCT.Domain.Entities;

namespace BCT.Domain;
public interface ICalculation
{
    List<DoubleValue> CalculateProjectValues(Project project, List<DoubleValue> storedValues, int startYear, int horizon);

    List<StringValue> GetAdditionalCriteria(List<BoolValue> enabled, List<DoubleValue> weights, List<StringValue> texts);
}
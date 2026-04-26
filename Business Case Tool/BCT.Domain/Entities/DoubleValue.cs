namespace BCT.Domain.Entities;
public class DoubleValue : IdModel, ValueModel
{
    public required int ProjectId { get; set; }
    public double Value { get; set; }
    public required string Key { get; set; }
    public int? Year { get; set; }

    public dynamic? DynamicValue { get => Value; set => Value = value; }
}

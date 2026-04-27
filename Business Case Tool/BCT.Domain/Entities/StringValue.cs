namespace BCT.Domain.Entities;
public class StringValue : IdModel, ValueModel
{
    public required int ProjectId { get; set; }
    public string? Value { get; set; }
    public required string Key { get; set; }
    public int? Year { get; set; }

    public dynamic? DynamicValue { get => Value; set => Value = value; }
}

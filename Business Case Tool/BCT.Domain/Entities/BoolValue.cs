namespace BCT.Domain.Entities;
public class BoolValue : IdModel, ValueModel
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public bool Value { get; set; }
    public string Key { get; set; } = string.Empty;
    public int? Year { get; set; }

    public dynamic? DynamicValue { get => Value; set => Value = value; }
}

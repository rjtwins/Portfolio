namespace BCT.Domain.Entities;
public interface ValueModel
{
    public int ProjectId { get; set; }
    public string Key { get; set; }
    public int? Year { get; set; }

    public dynamic? DynamicValue { get; set; }
}


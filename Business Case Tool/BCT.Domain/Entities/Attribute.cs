namespace BCT.Domain.Entities;
public class Attribute : IdModel
{
    public required string Tags { get; set; }
    public required string Key { get; set; }
}

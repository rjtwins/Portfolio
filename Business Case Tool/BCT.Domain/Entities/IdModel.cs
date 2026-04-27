namespace BCT.Domain.Entities;

public abstract class IdModel
{
	public int Id { get; set; }
    public DateTime RecordCreatedAt { get; set; }
    public DateTime RecordUpdatedAt { get; set; }
}

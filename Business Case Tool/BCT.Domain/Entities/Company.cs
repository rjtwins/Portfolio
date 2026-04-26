namespace BCT.Domain.Entities;

public class Company : IdModel
{
	public required int CreatorId { get; set; }
	public User? Creator { get; set; }
	public List<User> Users { get; set; } = new List<User>();
	public List<Project> Projects { get; set; } = new List<Project>();
    public List<Tag> Tags { get; set; } = new List<Tag>();
    public required string Name {get; set;}
    public string Description { get; set; } = string.Empty;
    public string BtwNumber { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}
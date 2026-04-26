namespace BCT.Domain.Entities;

public class Project : IdModel
{
	public string Name { get; set; } = string.Empty;
	public int CompanyId { get; set; }
	public Company Company { get; set; }
    public List<Tag> Tags { get; set; } = new();

    public int StartYear { get; set; } = DateTime.Now.Year;
    public int Horizon { get; set; } = 5;
    public bool InterestEnabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;

    public int ChosenGridMethod { get; set; } = 0;
}
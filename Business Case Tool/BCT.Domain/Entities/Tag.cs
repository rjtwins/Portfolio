using System.Collections;

namespace BCT.Domain.Entities;
public class Tag : IdModel
{
    public required string Text { get; set; }
    public required int CompanyId { get; set; }
    public required Company Company { get; set; }
    public List<Project> Projects { get; set; } = new();

    public override string ToString()
    {
        return Text;
    }
}

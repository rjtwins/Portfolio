using System;

namespace BCT.Domain.Entities;

public class Role
{
	public required string Name { get; set; }
	public required string Auth0Id { get; set; }
	public string Description { get; set; } = string.Empty;
}

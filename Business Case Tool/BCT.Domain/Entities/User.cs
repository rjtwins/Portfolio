namespace BCT.Domain.Entities;

public class User : IdModel
{
	public string Name { get; set; } = string.Empty;
	public required string AuthId { get; set; }
	public List<Company> Companies { get; set; } = new();
	public List<Company> CreatedCompanies { get; set; } = new();	
	public List<Role> Roles { get; set; } = new();
	public string? Email { get; set; } 
	public DateTime? UpdatedAt { get; set; }
	public bool? EmailVerified { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? LastLogin { get; set; }
	public string? LastIP { get; set; }
	public int? LoginCount { get; set; }


    public int? LastProjectId { get; set; }
    public int? LastCompanyId { get; set; }

    //Domain rules:
    public bool CanCreateUsers()
    {
        if(Roles.Any(x => x.Name == Const.Role.Admin))
            return true;

        if(Roles.Any(x => x.Name == Const.Role.Coach))
            return true;

        return false;
    }

    public bool CanBeDeletedByUser(User user)
    {
        if(user.Roles.Any(x => x.Name == Const.Role.Admin))
            return true;

        return false;
    }

    public bool CanAssignRole(Role role)
    {
        return CanAssignRoles().Contains(role.Name);
    }

    public bool CanRemoveRole(Role role)
    {
        return CanRemoveRoles().Contains(role.Name);
    }

    public string[] CanAssignRoles()
    {
        if (Roles.Any(x => x.Name == Const.Role.Admin))
            return new string[] { Const.Role.Admin, Const.Role.Coach };

        if (Roles.Any(x => x.Name == Const.Role.Coach))
            return new string[] { Const.Role.Coach };

        return Array.Empty<string>();
    }

    public string[] CanRemoveRoles()
    {
        return CanAssignRoles();
    }

    public bool HasRoleByName(string roleName)
    {
        return Roles.Select(x => x.Name).Contains(roleName);
    }

    public bool HasRoleByAuthId(string roleAuthId)
    {
        return Roles.Select(x => x.Auth0Id).Contains(roleAuthId);
    }
}

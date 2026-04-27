namespace BCT.Application;

internal static class DomainExtensions
{
	public static Domain.Entities.Role ToDomainEntity(this AuthEntities.AuthRole role)
	{
		return new Domain.Entities.Role()
		{
			Auth0Id = role.id,
			Name = role.name,
			Description = role.description
		};
	}

	public static void UpdateFrom(this Domain.Entities.User user, AuthEntities.AuthUser auth0User)
	{
		user.AuthId = auth0User.user_id;
		user.Email = auth0User.email;
		user.UpdatedAt = DateTime.Parse(auth0User.updated_at);
		user.EmailVerified = auth0User.email_verified;
		user.CreatedAt = DateTime.Parse(auth0User.created_at);
		user.LastLogin = DateTime.Parse(auth0User.last_login ?? DateTime.UtcNow.ToLongDateString());
		user.LastIP = auth0User.last_ip;
		user.LoginCount = auth0User.login_counts;
	}
}

namespace BCT.Application.AuthEntities;

public class AuthUser
{
	public required string user_id { get; set; }
	public string? picture { get; set; }
	public string? updated_at { get; set; }
	public string? nickname { get; set; }
	public bool? email_verified { get; set; }
	public string? created_at { get; set; }
	public string? name { get; set; }
	public string? email { get; set; }
	public string? last_login { get; set; }
	public string? last_ip { get; set; }
	public int? login_counts { get; set; }

	public AuthUserMetaData user_metadata { get; set; }
}

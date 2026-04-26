using System.Text.Json.Serialization;

namespace BCT.Application.AuthEntities;

public class AuthToken
{

	public string access_token { get; set; } = string.Empty;
	public string scope { get; set; } = string.Empty;
	public int expires_in { get; set; } = 0;
	public string token_type { get; set; } = string.Empty;
	
	//Helper methods:
	[JsonIgnore]
	public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
	[JsonIgnore]
	public DateTime ExpiresAt => IssuedAt + TimeSpan.FromSeconds(expires_in);
	[JsonIgnore]
	public bool Expired => ExpiresAt <= DateTime.UtcNow;
	[JsonIgnore]
	public bool NearlyExpired => ExpiresAt - TimeSpan.FromHours(8) <= DateTime.UtcNow;
		
	public void Refresh(AuthToken newToken)
	{
		access_token = newToken.access_token;
		scope = newToken.scope;
		expires_in = newToken.expires_in;
		token_type = newToken.token_type;
		IssuedAt = DateTime.UtcNow;
	}
}

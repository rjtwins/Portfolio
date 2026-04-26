namespace BCT.Application.AuthEntities;

public class AuthUserCreateResponse
{
    public DateTime created_at { get; set; }
    public string email { get; set; }
    public bool email_verified { get; set; }
    public string name { get; set; }
    public string nickname { get; set; }
    public string picture { get; set; }
    public DateTime updated_at { get; set; }
    public string user_id { get; set; }
}
namespace BCT.Application.AuthEntities;

public class AuthUserCreateModel
{
    public string email { get; set; } = string.Empty;
    public string connection { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;

    #region ignored for now
    //public string phone_number { get; set; } = string.Empty;
    ////public UserMetadata user_metadata { get; set; } = null;
    //public bool blocked { get; set; } = false;
    //public bool email_verified { get; set; } = true;
    //public bool phone_verified { get; set; } = false;
    //public string given_name { get; set; } = string.Empty;
    //public string family_name { get; set; } = string.Empty;
    //public string name { get; set; } = string.Empty;
    //public string nickname { get; set; } = string.Empty;
    ////public string picture { get; set; } = null;
    //public string user_id { get; set; } = string.Empty;
    //public bool verify_email { get; set; } = false;
    //public string username { get; set; } = string.Empty;
    #endregion
}

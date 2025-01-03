namespace API.Auth.Options;

public class MicrosoftSettings
{
    // App
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scopes { get; set; }
    public string RedirectUri { get; set; }
    // Auth
    public string RefreshToken { get; set; }
    public string AccessTokenValue { get; set; }
    public string AccessTokenExpiresOn { get; set; }
}
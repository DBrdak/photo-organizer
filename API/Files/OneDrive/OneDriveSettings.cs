namespace API.Files.OneDrive;

public class OneDriveSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RefreshToken  { get; set; }
    public string UserId { get; set; }
    public string RedirectUri { get; set; }
    public string Scopes { get; set; }
}
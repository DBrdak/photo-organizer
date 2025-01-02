using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace API.Files.OneDrive;

internal sealed class OneDriveSettingsSetup : IPostConfigureOptions<OneDriveSettings>
{
    private readonly IConfiguration _configuration;

    public OneDriveSettingsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void PostConfigure(string? name, OneDriveSettings options)
    {
        options.ClientId = _configuration["microsoft:client-id"] ?? throw new ArgumentException("microsoft ClientId is not set");
        options.ClientSecret = _configuration["microsoft:client-secret"] ?? throw new ArgumentException("microsoft ClientSecret is not set");
        options.RefreshToken = _configuration["microsoft:refresh-token"] ?? throw new ArgumentException("microsoft Refresh Token is not set");
        options.UserId = _configuration["microsoft:user-id"] ?? throw new ArgumentException("microsoft User Id is not set");
        options.RedirectUri = _configuration["microsoft:redirect-uri"] ?? throw new ArgumentException("microsoft Redirect Uri is not set");
        options.Scopes = _configuration["microsoft:scopes"] ?? throw new ArgumentException("microsoft Scopes is not set");
    }
}
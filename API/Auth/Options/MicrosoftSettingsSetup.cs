using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace API.Auth.Options;

internal sealed class MicrosoftSettingsSetup : IPostConfigureOptions<MicrosoftSettings>
{
    private readonly IConfiguration _configuration;

    public MicrosoftSettingsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void PostConfigure(string? name, MicrosoftSettings options)
    {
        options.ClientId = _configuration["microsoft:client-id"] ?? throw new ArgumentException("Microsoft ClientId is not set");
        options.ClientSecret = _configuration["microsoft:client-secret"] ?? throw new ArgumentException("Microsoft ClientSecret is not set");
        options.RefreshToken = _configuration["microsoft:refresh-token"] ?? throw new ArgumentException("Microsoft Refresh Token is not set");
        options.AccessTokenValue = _configuration["microsoft:access-token:value"] ?? throw new ArgumentException("Microsoft Access Token Values is not set");
        options.AccessTokenExpiresOn = _configuration["microsoft:access-token:expires-on"] ?? throw new ArgumentException("Microsoft Access Token Expiry Date is not set");
        options.RedirectUri = _configuration["microsoft:redirect-uri"] ?? throw new ArgumentException("Microsoft Redirect Uri is not set");
        options.Scopes = _configuration["microsoft:scopes"] ?? throw new ArgumentException("Microsoft Scopes is not set");
    }
}
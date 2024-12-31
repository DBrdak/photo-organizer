using Amazon.SimpleSystemsManagement.Model;
using Amazon.SimpleSystemsManagement;
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
        options.ClientId = _configuration["one-drive:client-id"] ?? throw new ArgumentException("OneDrive ClientId is not set");
        options.ClientSecret = _configuration["one-drive:client-secret"] ?? throw new ArgumentException("OneDrive ClientSecret is not set");
        options.ClientSecret = _configuration["one-drive:user-id"] ?? throw new ArgumentException("OneDrive UserId is not set");
    }
}
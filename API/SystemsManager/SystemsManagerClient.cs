using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace API.SystemsManager;

public sealed class SystemsManagerClient
{
    private readonly IAmazonSimpleSystemsManagement _ssm;
    private const string microsoftRefreshTokenName = "/photo-organizer/microsoft/refresh-token";

    public SystemsManagerClient(IAmazonSimpleSystemsManagement ssm)
    {
        _ssm = ssm;
    }

    public async Task SetMicrosoftRefreshTokenAsync(string refreshToken) =>
        await SetParameterAsync(microsoftRefreshTokenName, refreshToken);

    public async Task SetParameterAsync(string name, string value) =>
        await _ssm.PutParameterAsync(new PutParameterRequest
        {
            Name = name,
            Value = value,
            Type = ParameterType.String,
            Overwrite = true
        });

    public async Task SetMicrosoftAccessTokenAsync(string accessToken, long expiresOn)
    {
        await SetParameterAsync("/photo-organizer/microsoft/access-token/value", accessToken);
        await SetParameterAsync("/photo-organizer/microsoft/access-token/expires-on", expiresOn.ToString());
    }
}
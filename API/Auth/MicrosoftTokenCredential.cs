using System.Net.Http.Headers;

namespace API.Auth;

public sealed class MicrosoftTokenCredential
{
    private readonly MicrosoftAuthenticationService _authService;

    public MicrosoftTokenCredential(MicrosoftAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task AddTokenToHeadersAsync(HttpClient client)
    {
        var tokenResult = await _authService.GenerateNewAccessTokenAsync();

        if (tokenResult.IsFailure)
        {
            throw new Exception(tokenResult.Error);
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResult.Value.Token);
    }
}
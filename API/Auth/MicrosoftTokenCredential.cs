using Azure.Core;

namespace API.Auth;

public sealed class MicrosoftTokenCredential : TokenCredential
{
    private readonly MicrosoftAuthenticationService _authService;

    public MicrosoftTokenCredential(MicrosoftAuthenticationService authService)
    {
        _authService = authService;
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var tokenResult = await _authService.GenerateNewAccessTokenAsync();

        if (tokenResult.IsFailure)
        {
            throw new Exception(tokenResult.Error);
        }

        return new AccessToken(tokenResult.Value.Token, tokenResult.Value.ExpiresOn);
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var tokenResult = _authService.GenerateNewAccessTokenAsync().Result;

        if (tokenResult.IsFailure)
        {
            throw new Exception(tokenResult.Error);
        }

        return new AccessToken(tokenResult.Value.Token, tokenResult.Value.ExpiresOn);
    }
}
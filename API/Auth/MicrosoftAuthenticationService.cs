using API.Domain.Responses;
using API.SystemsManager;
using Newtonsoft.Json;
using System.Web;
using API.Auth.Models;
using Microsoft.Extensions.Options;
using API.Auth.Options;

namespace API.Auth;

public sealed class MicrosoftAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly MicrosoftSettings _settings;
    private readonly SystemsManagerClient _ssm;

    public MicrosoftAuthenticationService(IOptions<MicrosoftSettings> settings, SystemsManagerClient ssm)
    {
        _ssm = ssm;
        _settings = settings.Value;
        _httpClient = new HttpClient();
    }

    public string GenerateAuthorizationUrl()
    {
        var clientId = _settings.ClientId;
        var redirectUri = _settings.RedirectUri;
        var scopes = HttpUtility.UrlEncode(_settings.Scopes);

        return $"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?" +
               $"client_id={clientId}&" +
               $"response_type=code&" +
               $"redirect_uri={redirectUri}&" +
               $"response_mode=query&" +
               $"scope={scopes}";
    }

    public async Task<Result> ExchangeAuthorizationCodeForTokensAsync(string authorizationCode)
    {
        try
        {
            var clientId = _settings.ClientId;
            var clientSecret = _settings.ClientSecret;
            var redirectUri = _settings.RedirectUri;

            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["scope"] = _settings.Scopes,
                ["code"] = authorizationCode,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
                ["client_secret"] = clientSecret
            });

            var response = await _httpClient.PostAsync(
                "https://login.live.com/oauth20_token.srf",
                tokenRequest);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure($"Failed to exchange authorization code for tokens, error: {content}");
            }

            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            if (tokenResponse is null)
            {
                return Result.Failure<AccessToken>($"Failed to exchange refresh token for access token, error: {content}");
            }

            _ = tokenResponse.TryGetValue("refresh_token", out var refreshToken);

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result.Failure<AccessToken>("No tokens in response");
            }

            await _ssm.SetMicrosoftRefreshTokenAsync(refreshToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error during authorization code exchange: {ex.Message}");
        }
    }

    public async Task<Result> GenerateNewRefreshToken()
    {
        try
        {
            var clientId = _settings.ClientId;
            var clientSecret = _settings.ClientSecret;
            var redirectUri = _settings.RedirectUri;

            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["scope"] = _settings.Scopes,
                ["refresh_token"] = _settings.RefreshToken,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "refresh_token",
                ["client_secret"] = clientSecret
            });

            var response = await _httpClient.PostAsync(
                "https://login.live.com/oauth20_token.srf",
                tokenRequest);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<string>($"Failed to generate new refresh token, error: {content}");
            }

            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            if (tokenResponse is null)
            {
                return Result.Failure<AccessToken>($"Failed to exchange refresh token for access token, error: {content}");
            }

            _ = tokenResponse.TryGetValue("refresh_token", out var refreshToken);

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result.Failure<AccessToken>("No tokens in response");
            }

            await _ssm.SetMicrosoftRefreshTokenAsync(refreshToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error during refresh token exchange: {ex.Message}");
        }
    }

    public async Task<Result<AccessToken>> GenerateNewAccessTokenAsync()
    {
        try
        {
            if(!string.IsNullOrWhiteSpace(_settings.AccessTokenValue) &&
               long.Parse(_settings.AccessTokenExpiresOn) > DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 30)
            {
                return Result.Success(
                    new AccessToken(
                        _settings.AccessTokenValue,
                        DateTimeOffset.FromUnixTimeSeconds(long.Parse(_settings.AccessTokenExpiresOn))));
            }

            var clientId = _settings.ClientId;
            var clientSecret = _settings.ClientSecret;
            var redirectUri = _settings.RedirectUri;

            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["scope"] = _settings.Scopes,
                ["refresh_token"] = _settings.RefreshToken,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "refresh_token",
                ["client_secret"] = clientSecret
            });

            var response = await _httpClient.PostAsync(
                "https://login.live.com/oauth20_token.srf",
                tokenRequest);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<AccessToken>($"Failed to exchange refresh token for access token, error: {content}");
            }

            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            if(tokenResponse is null)
            {
                return Result.Failure<AccessToken>($"Failed to exchange refresh token for access token, error: {content}");
            }

            _ = tokenResponse.TryGetValue("refresh_token", out var refreshToken);
            _ = tokenResponse.TryGetValue("access_token", out var accessToken);
            _ = tokenResponse.TryGetValue("expires_in", out var expiresIn);

            if (string.IsNullOrWhiteSpace(refreshToken) ||
                string.IsNullOrWhiteSpace(accessToken) ||
                string.IsNullOrWhiteSpace(expiresIn))
            {
                return Result.Failure<AccessToken>("No tokens in response");
            }

            var expirationDate = DateTimeOffset.UtcNow.AddSeconds(long.Parse(expiresIn));

            await _ssm.SetMicrosoftRefreshTokenAsync(refreshToken);
            await _ssm.SetMicrosoftAccessTokenAsync(accessToken, expirationDate.ToUnixTimeSeconds());

            return Result.Success(new AccessToken(accessToken, expirationDate));
        }
        catch (Exception ex)
        {
            return Result.Failure<AccessToken>($"Error during refresh token exchange: {ex.Message}");
        }

    }
}
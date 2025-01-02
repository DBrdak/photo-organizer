using API.Domain.Responses;
using API.SystemsManager;
using Newtonsoft.Json;
using System.Web;
using API.Files.OneDrive;
using Azure.Core;
using Microsoft.Extensions.Options;

namespace API.Auth;

public sealed class MicrosoftAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly OneDriveSettings _settings;
    private readonly SystemsManagerClient _ssm;

    public MicrosoftAuthenticationService(IOptions<OneDriveSettings> settings, SystemsManagerClient ssm)
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
            var refreshToken = string.Empty;
            var accessToken = string.Empty;
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
            var isRefreshTokenExists = tokenResponse is not null &&
                                tokenResponse.TryGetValue("refresh_token", out refreshToken);

            if (!isRefreshTokenExists || string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result.Failure("No tokens in response");
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
            var refreshToken = string.Empty;
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
            var isRefreshTokenExists = tokenResponse is not null &&
                                       tokenResponse.TryGetValue("refresh_token", out refreshToken);

            if (!isRefreshTokenExists || string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result.Failure("No refresh token in response");
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
            var refreshToken = string.Empty;
            var accessToken = string.Empty;
            var expiresIn = string.Empty;
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
            var isRefreshTokenExists = tokenResponse is not null &&
                                       tokenResponse.TryGetValue("refresh_token", out refreshToken);
            var isAccessTokenExists = tokenResponse is not null &&
                                       tokenResponse.TryGetValue("access_token", out accessToken);
            var isExpiresInExists = tokenResponse is not null &&
                                       tokenResponse.TryGetValue("expires_in", out expiresIn);

            if (!isRefreshTokenExists ||
                !isAccessTokenExists ||
                !isExpiresInExists ||
                string.IsNullOrWhiteSpace(refreshToken) ||
                string.IsNullOrWhiteSpace(accessToken) ||
                string.IsNullOrWhiteSpace(expiresIn))
            {
                return Result.Failure<AccessToken>("No tokens in response");
            }

            await _ssm.SetMicrosoftRefreshTokenAsync(refreshToken);

            var expirationDate = DateTime.UtcNow.AddSeconds(int.Parse(expiresIn));

            return Result.Success(new AccessToken(accessToken, expirationDate));
        }
        catch (Exception ex)
        {
            return Result.Failure<AccessToken>($"Error during refresh token exchange: {ex.Message}");
        }

    }
}
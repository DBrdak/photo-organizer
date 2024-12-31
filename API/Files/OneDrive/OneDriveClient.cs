using API.Domain;
using Microsoft.Identity.Client;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using API.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Files.OneDrive;

public sealed class OneDriveClient
{
    private readonly HttpClient _client;
    private readonly IConfidentialClientApplication _msalClient;
    private readonly OneDriveSettings _settings;

    public OneDriveClient(
        HttpClient client,
        IOptions<OneDriveSettings> options)
    {
        _client = client;
        _settings = options.Value;

        _msalClient = ConfidentialClientApplicationBuilder
            .Create(options.Value.ClientId)
            .WithClientSecret(options.Value.ClientSecret)
            .WithAuthority("https://login.microsoftonline.com/consumers/")
            .Build();
    }

    private async Task<Result<string>> GetAccessTokenAsync()
    {
        try
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var result = await _msalClient.AcquireTokenForClient(scopes).ExecuteAsync();
            return Result.Success(result.AccessToken);
        }
        catch (MsalException ex)
        {
            return Result.Failure<string>($"Authentication failed: {ex.Message}");
        }
    }

    public async Task<Result<string>> UploadPhotoAsync(PhotoObject photo)
    {
        var tokenResult = await GetAccessTokenAsync();
        if (tokenResult.IsFailure)
        {
            return tokenResult;
        }

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.Value);

        try
        {
            byte[] fileBytes = Convert.FromBase64String(photo.Base64Image);
            var sessionResult = await CreateUploadSessionAsync(photo);

            if (sessionResult.IsFailure)
            {
                return Result.Failure<string>(sessionResult.Error);
            }

            var uploadResult = await UploadFileInChunksAsync(sessionResult.Value.UploadUrl, fileBytes);
            if (uploadResult.IsFailure)
            {
                return Result.Failure<string>(uploadResult.Error);
            }

            return Result.Success($"https://onedrive.live.com/redir?resid={sessionResult.Value.ResourceId}");
        }
        catch (FormatException)
        {
            return Result.Failure<string>("Invalid base64 string provided");
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Unexpected error during upload: {ex.Message}");
        }
    }

    private async Task<Result<OneDriveUploadSession>> CreateUploadSessionAsync(PhotoObject photo)
    {
        var requestBody = new OneDriveUploadRequest();
        var createSessionContent = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var sessionResponse = await _client.PostAsync(
            $"/v1.0/users/{_settings.UserId}//drive/root:{photo.Path}/{photo.FileName}:/createUploadSession",
            createSessionContent
        );

        if (!sessionResponse.IsSuccessStatusCode)
        {
            return Result.Failure<OneDriveUploadSession>(
                $"Failed to create upload session: {await sessionResponse.Content.ReadAsStringAsync()}");
        }

        var sessionInfo = JsonConvert.DeserializeObject<OneDriveUploadSession>(
            await sessionResponse.Content.ReadAsStringAsync()
        );

        return Result.Success(sessionInfo);
    }

    private async Task<Result> UploadFileInChunksAsync(string uploadUrl, byte[] fileBytes)
    {
        const int chunkSize = 320 * 1024; // 320 KB chunks
        var totalLength = fileBytes.Length;

        for (var i = 0; i < totalLength; i += chunkSize)
        {
            var chunkLength = Math.Min(chunkSize, totalLength - i);
            var chunk = new byte[chunkLength];
            Array.Copy(fileBytes, i, chunk, 0, chunkLength);

            using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            request.Content = new ByteArrayContent(chunk);
            request.Content.Headers.ContentLength = chunkLength;
            request.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(
                i,
                i + chunkLength - 1,
                totalLength
            );

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure($"Failed to upload chunk: {await response.Content.ReadAsStringAsync()}");
            }
        }

        return Result.Success();
    }
}
using System.Web;
using Amazon.Lambda.Core;
using API.Auth;
using API.Domain;
using API.Domain.Responses;
using Newtonsoft.Json;

namespace API.Files.OneDrive;

public sealed class OneDriveClient
{
    private readonly HttpClient _client;
    private readonly MicrosoftTokenCredential _tokenCredential;

    public OneDriveClient(HttpClient client, MicrosoftTokenCredential tokenCredential)
    {
        _client = client;
        _tokenCredential = tokenCredential;
    }

    public async Task<Result> UploadPhotoAsync(PhotoObject photo)
    {
        try
        {
            await _tokenCredential.AddTokenToHeadersAsync(_client);

            return await UploadFileAsync(photo);
        }
        catch (FormatException)
        {
            return Result.Failure("Invalid base64 string provided");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Unexpected error during upload: {ex.Message}");
        }
    }

    private async Task<Result<string>> UploadFileAsync(PhotoObject photo)
    {
        try
        {
            var file = new MemoryStream(Convert.FromBase64String(photo.Base64Image));
            var requestUrl = $"me/drive/root:/{photo.Path}/{photo.FileName}:/content";
            requestUrl = HttpUtility.UrlEncode(requestUrl);

            var response = await _client.PutAsync(requestUrl, new StreamContent(file));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<string>(
                    $"[{response.StatusCode}] Failed to upload file: {responseContent}");
            }

            return Result.Success(responseContent);
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error during file upload: {ex}");
            return Result.Failure<string>($"Error during file upload: {ex.Message}");
        }
    }
}
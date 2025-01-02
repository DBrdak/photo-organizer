using Amazon.Lambda.Core;
using API.Auth;
using API.Domain;
using API.Domain.Responses;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;

namespace API.Files.OneDrive;

public sealed class OneDriveClient
{
    private readonly HttpClient _client;
    private readonly GraphServiceClient _graphClient;

    public OneDriveClient(HttpClient client, MicrosoftTokenCredential tokenCredential)
    {
        _client = client;
        _graphClient = new GraphServiceClient(tokenCredential);
    }

    public async Task<Result> UploadPhotoAsync(PhotoObject photo)
    {
        try
        {
            var fileBytes = Convert.FromBase64String(photo.Base64Image);
            var stream = new MemoryStream(fileBytes);

            // TODO Fix
            var uploadSession = await _graphClient
                .Drives[""]
                .Items[photo.FolderId]
                .ItemWithPath(photo.FileName)
                .Content
                .PutAsync(stream);

            await stream.DisposeAsync();

            LambdaLogger.Log($"Upload session URL: {JsonConvert.SerializeObject(uploadSession)}");

            return Result.Success();
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

    private async Task<Result<string>> UploadFileAsync(PhotoObject photo, byte[] fileBytes)
    {
        try
        {

            var contentType = "text/plain";

            var requestUrl = 
                $"/v1.0/me/drive/items/{photo.FolderId}:/{photo.FileName}:/content?@microsoft.graph.conflictBehavior=replace";

            using var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
            using var content = new StreamContent(new MemoryStream(fileBytes));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            request.Content = new StringContent(photo.Base64Image);

            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            LambdaLogger.Log($"Upload request URL: {requestUrl}");
            LambdaLogger.Log($"Upload response status: {response.StatusCode}");
            LambdaLogger.Log($"Upload response content: {responseContent}");

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
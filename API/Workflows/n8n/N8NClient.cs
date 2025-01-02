using System.Text;
using API.Domain;
using API.Domain.Responses;
using Newtonsoft.Json;

namespace API.Workflows.N8N;

public sealed class N8NClient
{
    private readonly HttpClient _client;

    public N8NClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<Result> UploadPhotoAsync(PhotoObject photoObject)
    {
        var content = new StringContent(JsonConvert.SerializeObject(photoObject), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/photos/new", content);

        if (response.IsSuccessStatusCode)
        {
            return Result.Success();
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return Result.Failure($"n8n failed to upload {photoObject.FileName}, error: {responseContent}");
    }
}
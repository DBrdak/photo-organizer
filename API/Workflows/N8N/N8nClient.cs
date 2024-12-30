using System.Text;
using API.Domain;
using Newtonsoft.Json;

namespace API.Workflows.N8N;

public sealed class N8NClient
{
    private readonly HttpClient _client;

    public N8NClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> UploadPhotoAsync(PhotoObject photoObject)
    {
        var content = new StringContent(JsonConvert.SerializeObject(photoObject), Encoding.UTF8, "application/json");

        return await _client.PostAsync("/upload", content); // TODO Handle response
    }
}
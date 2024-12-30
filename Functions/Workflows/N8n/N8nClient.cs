using System.Text;
using Functions.Domain;
using Newtonsoft.Json;

namespace Functions.Workflows.N8n;

internal sealed class N8nClient
{
    private readonly HttpClient _client;

    public N8nClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> UploadPhotoAsync(Photo photo)
    {
        var content = new StringContent(JsonConvert.SerializeObject(photo), Encoding.UTF8, "application/json");

        return await _client.PostAsync("", content);
    }
}
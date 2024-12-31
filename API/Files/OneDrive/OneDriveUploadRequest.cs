using Newtonsoft.Json;

namespace API.Files.OneDrive;

public sealed class OneDriveUploadRequest
{
    [JsonProperty("item")]
    public OneDriveUploadRequestItem Item { get; set; } = new();
}
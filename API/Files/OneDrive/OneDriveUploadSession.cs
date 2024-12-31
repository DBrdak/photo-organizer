using Newtonsoft.Json;

namespace API.Files.OneDrive;

public sealed class OneDriveUploadSession
{
    [JsonProperty("uploadUrl")]
    public string UploadUrl { get; set; }

    [JsonProperty("resourceId")]
    public string ResourceId { get; set; }
}
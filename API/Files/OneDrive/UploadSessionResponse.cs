using Newtonsoft.Json;

namespace API.Files.OneDrive;

public class UploadSessionResponse
{
    [JsonProperty("uploadUrl")]
    public string UploadUrl { get; set; }

    [JsonProperty("resourceId")]
    public string ResourceId { get; set; }
}
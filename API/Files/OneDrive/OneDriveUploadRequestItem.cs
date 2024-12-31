using Newtonsoft.Json;

namespace API.Files.OneDrive;

public sealed class OneDriveUploadRequestItem
{
    [JsonProperty("@microsoft.graph.conflictBehavior")]
    public string ConflictBehavior { get; set; } = "rename";
}
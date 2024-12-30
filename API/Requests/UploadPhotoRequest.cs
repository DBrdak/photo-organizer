namespace API.Requests;

public sealed record UploadPhotoRequest(string Name, string Extension, string File);
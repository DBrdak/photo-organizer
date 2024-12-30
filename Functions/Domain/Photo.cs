namespace Functions.Domain;

internal sealed record Photo
{
    public string FileName { get; set; }
    public string Base64Image { get; set; }
}
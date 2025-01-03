using API.Utils.Dates;

namespace API.Domain.Albums;

public sealed class Album
{
    public string Name { get; init; }
    public string Path { get; init; }
    public string FilePath { get; init; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; init; }
    private const string parentAlbumName = "Photos";

    public Album(string name, bool isActive, bool isDefault = false)
    {
        Name = name;
        IsActive = isActive;
        IsDefault = isDefault;
        Path = $"{parentAlbumName}/{Name}";
        FilePath = $"{parentAlbumName}/{Name}/{DateTimeProvider.WarsawNow:yyyy-MM-dd}";
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
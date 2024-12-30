﻿namespace API.Domain;

public sealed class Album
{
    public string Name { get; init; }
    public string Id { get; init; }
    public string Path { get; init; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; init; }

    public Album(string name, string id, string path, bool isActive, bool isDefault = false)
    {
        Name = name;
        Id = id;
        Path = path;
        IsActive = isActive;
        IsDefault = isDefault;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
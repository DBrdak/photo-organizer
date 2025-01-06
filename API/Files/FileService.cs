using Amazon.Lambda.Core;
using API.Data.Repositories;
using API.Domain;
using API.Domain.Albums;
using API.Domain.Responses;
using API.Files.OneDrive;
using API.Requests;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace API.Files;

public sealed class FileService
{
    private readonly AlbumsRepository _albumsRepository;
    private readonly OneDriveClient _oneDriveClient;

    public FileService(AlbumsRepository albumsRepository, OneDriveClient oneDriveClient)
    {
        _albumsRepository = albumsRepository;
        _oneDriveClient = oneDriveClient;
    }

    public async Task<Result> UploadPhotoAsync(UploadPhotoRequest request)
    {
        var album = await _albumsRepository.GetActiveAlbumAsync();

        var photoObject = new PhotoObject(
            $"{request.Name}{request.User.ToUpper()}.{request.Extension}",
            request.File,
            album.FilePath);

        return await _oneDriveClient.UploadPhotoAsync(photoObject);
    }

    public async Task SetActiveAlbumAsync(string name)
    {
        var album = await _albumsRepository.GetByNameAsync(name) ?? new Album(name, false);
        var activeAlbum = await _albumsRepository.GetActiveAlbumAsync();

        activeAlbum.Deactivate();
        album.Activate();
        await _albumsRepository.UpdateAlbumAsync(activeAlbum);
        await _albumsRepository.UpdateAlbumAsync(album);
    }

    public async Task ResetAlbumAsync()
    {
        var activeAlbum = await _albumsRepository.GetActiveAlbumAsync();
        var defaultAlbum = await _albumsRepository.GetDefaultAlbumAsync();

        activeAlbum.Deactivate();
        defaultAlbum.Activate();

        await _albumsRepository.UpdateAlbumAsync(activeAlbum);
        await _albumsRepository.UpdateAlbumAsync(defaultAlbum);
    }
}
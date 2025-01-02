using API.Data.Repositories;
using API.Domain;
using API.Domain.Responses;
using API.Files.OneDrive;
using API.Requests;

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
            $"{request.Name}.{request.Extension}",
            request.File,
            album.Id);

        return await _oneDriveClient.UploadPhotoAsync(photoObject);
    }

    public async Task SetAlbumAsync(string name)
    {
        var album = await _albumsRepository.GetByNameAsync(name);

        if (album is not null)
        {
            var activeAlbum = await _albumsRepository.GetActiveAlbumAsync();

            activeAlbum.Deactivate();
            album.Activate();

            await _albumsRepository.UpdateAlbumAsync(album);
            await _albumsRepository.UpdateAlbumAsync(activeAlbum);
            return;
        }

        // Create album via n8n then Fetch ID and create album
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
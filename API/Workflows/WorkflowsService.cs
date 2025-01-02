using API.Data.Repositories;
using API.Domain;
using API.Domain.Responses;
using API.Requests;
using API.Workflows.N8N;

namespace API.Workflows;

public sealed class WorkflowsService
{
    private readonly N8NClient _n8nClient;
    private readonly AlbumsRepository _albumsRepository;

    public WorkflowsService(N8NClient n8NClient, AlbumsRepository albumsRepository)
    {
        _n8nClient = n8NClient;
        _albumsRepository = albumsRepository;
    }

    public async Task<Result> UploadPhotoAsync(UploadPhotoRequest request)
    {
        var album = await _albumsRepository.GetActiveAlbumAsync();

        var photoObject = new PhotoObject(
            $"{request.Name}.{request.Extension}",
            request.File,
            album.Id);

        return await _n8nClient.UploadPhotoAsync(photoObject);
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
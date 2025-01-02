using Amazon.DynamoDBv2.DocumentModel;
using API.Domain;

namespace API.Data.Repositories;

public sealed class AlbumsRepository : Repository<Album>
{
    public AlbumsRepository(DataContext context) : base(context)
    {

    }

    public async Task<Album> GetDefaultAlbumAsync()
    {
        var filter = new ScanFilter();
        filter.AddCondition("IsDefault", ScanOperator.Equal, new DynamoDBBool(true));

        return await GetFirstOrDefaultAsync(filter) ?? 
               throw new ApplicationException("Default album not found");
    }

    public async Task AddAlbumAsync(Album album) =>
        await AddAsync(album);

    public async Task UpdateAlbumAsync(Album album) =>
        await UpdateAsync(album);

    public async Task<Album> GetActiveAlbumAsync()
    {
        var filter = new ScanFilter();
        filter.AddCondition("IsActive", ScanOperator.Equal, new DynamoDBBool(true));

        return await GetFirstOrDefaultAsync(filter) ??
               throw new ApplicationException("Active album not found");
    }

    public async Task<Album?> GetByNameAsync(string name) =>
        await GetByIdAsync(name);
}
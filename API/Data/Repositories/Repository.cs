using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;

namespace API.Data.Repositories;

public abstract class Repository<TEntity>
{
    protected readonly Table Table;
    protected readonly DataContext Context;

    protected Repository(DataContext context)
    {
        Context = context;
        Table = context.Set<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        var config = new ScanOperationConfig
        {
            Select = SelectValues.AllAttributes,
            Filter = new ScanFilter(),
            Limit = 1000,
        };
        
        var scanner = Table.Scan(config);
        var docs = new List<Document>();

        do 
            docs.AddRange(await scanner.GetNextSetAsync(cancellationToken));
        while (!scanner.IsDone);

        return docs.Select(
            doc =>
                JsonConvert.DeserializeObject<TEntity>(doc.ToJson()) ??
                throw new InvalidOperationException($"Couldn't deserialize document to {typeof(TEntity).Name}"));
    }

    public async Task<TEntity?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var doc = await Table.GetItemAsync(new Primitive(id), cancellationToken);

        return doc is null
            ? default
            : JsonConvert.DeserializeObject<TEntity>(doc.ToJson()) ??
              throw new InvalidOperationException($"Couldn't deserialize document to {typeof(TEntity).Name}");
    }

    protected virtual async Task<IEnumerable<TEntity>> GetWhereAsync(
        ScanFilter filter,
        CancellationToken cancellationToken = default)
    {
        var scanner = Table.Scan(filter);
        var docs = new List<Document>();

        do
            docs.AddRange(await scanner.GetNextSetAsync(cancellationToken));
        while (!scanner.IsDone);

        return docs.Select(
            doc =>
                JsonConvert.DeserializeObject<TEntity>(doc.ToJson()) ??
                throw new InvalidOperationException($"Couldn't deserialize document to {typeof(TEntity).Name}"));
    }

    protected virtual async Task<TEntity?> GetFirstOrDefaultAsync(
        ScanFilter filter,
        CancellationToken cancellationToken = default)
    {
        var scanner = Table.Scan(filter);

        var docs = await scanner.GetNextSetAsync(cancellationToken);

        var doc = docs.FirstOrDefault();

        return doc is null
            ? default
            : JsonConvert.DeserializeObject<TEntity>(doc.ToJson()) ??
              throw new InvalidOperationException($"Couldn't deserialize document to {typeof(TEntity).Name}");

    }

    public async Task<IEnumerable<TEntity>> GetManyByIdAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        var batch = Table.CreateBatchGet();

        ids.ToList().ForEach(id => batch.AddKey(new Primitive(id.ToString())));

        await batch.ExecuteAsync(cancellationToken);

        var docs = batch.Results;

        return docs.Select(
            doc =>
                JsonConvert.DeserializeObject<TEntity>(doc.ToJson()) ??
                throw new InvalidOperationException($"Couldn't deserialize document to {typeof(TEntity).Name}"));
    }

    public async Task RemoveAsync(
        string entityId, 
        CancellationToken cancellationToken = default)
    {
        await Table.DeleteItemAsync(new Primitive(entityId), cancellationToken);
    }

    public async Task RemoveRangeAsync(
        IEnumerable<string> entitiesId,
        CancellationToken cancellationToken = default)
    {
        var batch = Table.CreateBatchWrite();

        entitiesId.ToList().ForEach(id => batch.AddKeyToDelete(new Primitive(id)));

        await batch.ExecuteAsync(cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        var json = JsonConvert.SerializeObject(entity);
        var doc = Document.FromJson(json);

        await Table.PutItemAsync(doc, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        var batch = Table.CreateBatchWrite();

        var jsons = entities.Select(entity => JsonConvert.SerializeObject(entity));
        var docs = jsons.Select(Document.FromJson);

        docs.ToList().ForEach(batch.AddDocumentToPut);

        await batch.ExecuteAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task UpdateRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await AddRangeAsync(entities, cancellationToken);
    }
}
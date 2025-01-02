using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace API.Data;

public sealed class DataContext
{
    private readonly AmazonDynamoDBClient _client = new();
    public AmazonDynamoDBClient Client => _client;

    private readonly AmazonDynamoDBException _connectionException =
        new("Could not connect to DynamoDB");
    private AmazonDynamoDBException InvalidTableException(string typeName) =>
        new($"Table for {typeName} does not exist");

    private Table Albums => Table.TryLoadTable(_client, nameof(Albums), out var table) ?
        table : throw _connectionException;

    public Table Set<TEntity>() =>
        typeof(TEntity) switch
        {
            { Name: "Album" } => Albums,
            var type => throw InvalidTableException(type.Name)
        };
}
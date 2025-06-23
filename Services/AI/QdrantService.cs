using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Vector;
namespace UniTrackRemaster.Services.AI;

public class QdrantService : IQdrantService
{
    private readonly QdrantClient _client;
    private readonly ILogger<QdrantService> _logger;

    public QdrantService(QdrantClient client, ILogger<QdrantService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> CollectionExistsAsync(string collectionName)
    {
        try
        {
            var collections = await _client.ListCollectionsAsync();
            return collections.Contains(collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if collection {CollectionName} exists", collectionName);
            return false;
        }
    }

    public async Task CreateCollectionAsync(string collectionName, VectorParams vectorParams)
    {
        try
        {
            var distance = vectorParams.Distance switch
            {
                Distance.Cosine => Distance.Cosine,
                Distance.Euclid => Distance.Euclid,
                Distance.Dot => Distance.Dot,
                _ => Distance.Cosine
            };

            await _client.CreateCollectionAsync(collectionName, new VectorParams
            {
                Size = vectorParams.Size,
                Distance = distance
            });

            _logger.LogInformation("Successfully created collection {CollectionName}", collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create collection {CollectionName}", collectionName);
            throw;
        }
    }

    public async Task<UpsertResponse> UpsertPointsAsync(string collectionName, IEnumerable<PointStruct> points)
    {
        try
        {
            var response = await _client.UpsertAsync(collectionName, points.ToArray());

            _logger.LogInformation("Successfully upserted {Count} points to collection {CollectionName}",
                points.Count(), collectionName);

            return new UpsertResponse
            {
                Success = response.Status == UpdateStatus.Completed,
                UpsertedCount = points.Count(),
                Message = "Points upserted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert points to collection {CollectionName}", collectionName);
            return new UpsertResponse
            {
                Success = false,
                Message = ex.Message,
                UpsertedCount = 0
            };
        }
    }

    public async Task<List<ScoredPoint>> SearchAsync(string collectionName, float[] vector, uint limit = 10, Filter? filter = null)
    {
        try
        {
            var qdrantFilter = filter != null ? ConvertToQdrantFilter(filter) : null;

            var results = await _client.SearchAsync(
                collectionName: collectionName,
                vector: vector.AsMemory(),
                filter: qdrantFilter,
                limit: limit,
                payloadSelector: true  // Include all payload
            );

            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search in collection {CollectionName}", collectionName);
            return new List<ScoredPoint>();
        }
    }

    public async Task<List<RetrievedPoint>> GetPointsAsync(string collectionName, PointId pointId)
    {
        try
        {
            _logger.LogDebug("Retrieving point {PointId} from collection {CollectionName}", pointId, collectionName);

            var results = await _client.RetrieveAsync(
                collectionName: collectionName,
                ids: new[] { pointId },
                payloadSelector: true,  // Include all payload
                vectorSelector: true    // Include vectors
            );

            if (results == null || !results.Any())
            {
                _logger.LogWarning("Point {PointId} not found in collection {CollectionName}", pointId, collectionName);
                return new List<RetrievedPoint>();
            }

            _logger.LogDebug("Successfully retrieved {Count} points from collection {CollectionName}", 
                results.Count, collectionName);

            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get point {PointId} from collection {CollectionName}", 
                pointId, collectionName);
            return new List<RetrievedPoint>();
        }
    }

    public async Task<List<RetrievedPoint>> GetPointsAsync(string collectionName, IEnumerable<PointId> pointIds)
    {
        try
        {
            var ids = pointIds.ToArray();
            _logger.LogDebug("Retrieving {Count} points from collection {CollectionName}", 
                ids.Length, collectionName);

            var results = await _client.RetrieveAsync(
                collectionName: collectionName,
                ids: ids,
                payloadSelector: true,  // Include all payload
                vectorSelector: true    // Include vectors
            );

            if (results == null || !results.Any())
            {
                _logger.LogWarning("No points found in collection {CollectionName}", collectionName);
                return new List<RetrievedPoint>();
            }

            _logger.LogDebug("Successfully retrieved {Count} points from collection {CollectionName}", 
                results.Count, collectionName);

            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get points from collection {CollectionName}", collectionName);
            return new List<RetrievedPoint>();
        }
    }

    private Filter ConvertToQdrantFilter(Filter filter)
    {
        var qdrantFilter = new Filter();

        if (filter.Must.Any())
        {
            foreach (var condition in filter.Must)
            {
                var qdrantCondition = ConvertToQdrantCondition(condition);
                if (qdrantCondition != null)
                {
                    qdrantFilter.Must.Add(qdrantCondition);
                }
            }
        }

        if (filter.MustNot.Any())
        {
            foreach (var condition in filter.MustNot)
            {
                var qdrantCondition = ConvertToQdrantCondition(condition);
                if (qdrantCondition != null)
                {
                    qdrantFilter.MustNot.Add(qdrantCondition);
                }
            }
        }

        if (filter.Should.Any())
        {
            foreach (var condition in filter.Should)
            {
                var qdrantCondition = ConvertToQdrantCondition(condition);
                if (qdrantCondition != null)
                {
                    qdrantFilter.Should.Add(qdrantCondition);
                }
            }
        }

        return qdrantFilter;
    }

    private Condition? ConvertToQdrantCondition(Condition condition)
    {
        var qdrantCondition = new Condition();
        if (condition.Field != null)
        {
            var fieldCondition = new FieldCondition
            {
                Key = condition.Field.Key
            };

            if (condition.Field.Match != null)
            {
                fieldCondition.Match = new Match
                {
                    Text = condition.Field.Match?.ToString()
                };
            }

            if (condition.Field.Range != null)
            {
                var range = new Qdrant.Client.Grpc.Range();

                // Check against default value (0) or use double.NaN as "not set"
                if (condition.Field.Range.Gte != 0)
                    range.Gte = condition.Field.Range.Gte;
                if (condition.Field.Range.Gt != 0)
                    range.Gt = condition.Field.Range.Gt;
                if (condition.Field.Range.Lte != 0)
                    range.Lte = condition.Field.Range.Lte;
                if (condition.Field.Range.Lt != 0)
                    range.Lt = condition.Field.Range.Lt;

                fieldCondition.Range = range;
            }
            qdrantCondition.Field = fieldCondition;
            return qdrantCondition;
        }
        return null;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
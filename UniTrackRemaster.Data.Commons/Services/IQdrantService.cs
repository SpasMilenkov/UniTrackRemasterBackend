using Qdrant.Client.Grpc;
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Commons.Services;

public interface IQdrantService
{
    Task<bool> CollectionExistsAsync(string collectionName);
    Task CreateCollectionAsync(string collectionName, VectorParams vectorParams);
    Task<UpsertResponse> UpsertPointsAsync(string collectionName, IEnumerable<PointStruct> points);
    Task<List<ScoredPoint>> SearchAsync(string collectionName, float[] vector, uint limit = 10, Filter? filter = null);
    Task<List<RetrievedPoint>> GetPointsAsync(string collectionName, IEnumerable<PointId> pointIds);
}
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Commons.Services;

public interface IEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(string text);
    public Task<float[]> GenerateEmbeddingAsync(AnalyticsDocument document);

}

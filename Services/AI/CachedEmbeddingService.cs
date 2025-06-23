using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Services.AI;

public class CachedEmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingService _innerService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

    public CachedEmbeddingService(IEmbeddingService innerService, IMemoryCache cache)
    {
        _innerService = innerService;
        _cache = cache;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var cacheKey = GenerateCacheKey(text);

        if (_cache.TryGetValue(cacheKey, out float[]? cachedEmbedding))
        {
            return cachedEmbedding!;
        }
        var embedding = await _innerService.GenerateEmbeddingAsync(text);

        _cache.Set(cacheKey, embedding, _cacheExpiration);

        return embedding;
    }

    public async Task<float[]> GenerateEmbeddingAsync(AnalyticsDocument document)
    {
        // Use document ID + hash of content for caching
        var contentHash = GenerateContentHash(document);
        var cacheKey = $"doc_{document.Id}_{contentHash}";

        if (_cache.TryGetValue(cacheKey, out float[]? cachedEmbedding))
        {
            return cachedEmbedding!;
        }

        var embedding = await _innerService.GenerateEmbeddingAsync(document);

        _cache.Set(cacheKey, embedding, _cacheExpiration);

        return embedding;
    }

    private static string GenerateCacheKey(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash)[..16]; // Use first 16 chars
    }

    private static string GenerateContentHash(AnalyticsDocument document)
    {
        var contentString = $"{document.Content.ExecutiveSummary}{string.Join("", document.Content.KeyAchievements)}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(contentString));
        return Convert.ToHexString(hash)[..16];
    }
}
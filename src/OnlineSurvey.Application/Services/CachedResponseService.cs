using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Interfaces;

namespace OnlineSurvey.Application.Services;

public class CachedResponseService : IResponseService
{
    private readonly ResponseService _inner;
    private readonly ICacheService _cache;
    private const string ResultsCachePrefix = "survey_results_";
    private const string CountCachePrefix = "survey_count_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    public CachedResponseService(ResponseService inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Guid> SubmitResponseAsync(SubmitResponseRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var result = await _inner.SubmitResponseAsync(request, ipAddress, cancellationToken);

        // Invalidate cache when new response is submitted
        await _cache.RemoveAsync($"{ResultsCachePrefix}{request.SurveyId}", cancellationToken);
        await _cache.RemoveAsync($"{CountCachePrefix}{request.SurveyId}", cancellationToken);

        return result;
    }

    public async Task<SurveyResultResponse> GetSurveyResultsAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ResultsCachePrefix}{surveyId}";

        var cached = await _cache.GetAsync<SurveyResultResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var results = await _inner.GetSurveyResultsAsync(surveyId, cancellationToken);
        await _cache.SetAsync(cacheKey, results, CacheDuration, cancellationToken);

        return results;
    }

    public async Task<int> GetResponseCountAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CountCachePrefix}{surveyId}";

        var cached = await _cache.GetAsync<int?>(cacheKey, cancellationToken);
        if (cached.HasValue)
            return cached.Value;

        var count = await _inner.GetResponseCountAsync(surveyId, cancellationToken);
        await _cache.SetAsync(cacheKey, count, CacheDuration, cancellationToken);

        return count;
    }
}

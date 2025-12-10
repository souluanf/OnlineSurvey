using OnlineSurvey.Application.DTOs;

namespace OnlineSurvey.Application.Interfaces;

public interface IResponseService
{
    Task<Guid> SubmitResponseAsync(SubmitResponseRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<SurveyResultResponse> GetSurveyResultsAsync(Guid surveyId, CancellationToken cancellationToken = default);
    Task<int> GetResponseCountAsync(Guid surveyId, CancellationToken cancellationToken = default);
}

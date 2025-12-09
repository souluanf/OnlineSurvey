using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Domain.Repositories;

public interface IResponseRepository : IRepository<Response>
{
    Task<IEnumerable<Response>> GetBySurveyIdAsync(Guid surveyId, CancellationToken cancellationToken = default);
    Task<int> GetResponseCountBySurveyIdAsync(Guid surveyId, CancellationToken cancellationToken = default);
    Task<bool> HasRespondedAsync(Guid surveyId, string participantId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetOptionCountsAsync(Guid surveyId, CancellationToken cancellationToken = default);
}

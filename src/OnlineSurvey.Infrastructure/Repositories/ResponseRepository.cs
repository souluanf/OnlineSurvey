using Microsoft.EntityFrameworkCore;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Repositories;
using OnlineSurvey.Infrastructure.Data;

namespace OnlineSurvey.Infrastructure.Repositories;

public class ResponseRepository : Repository<Response>, IResponseRepository
{
    public ResponseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Response>> GetBySurveyIdAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Answers)
            .Where(r => r.SurveyId == surveyId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetResponseCountBySurveyIdAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(r => r.SurveyId == surveyId, cancellationToken);
    }

    public async Task<bool> HasRespondedAsync(Guid surveyId, string participantId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            r => r.SurveyId == surveyId && r.ParticipantId == participantId,
            cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetOptionCountsAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        return await Context.Answers
            .Join(Context.Responses,
                a => a.ResponseId,
                r => r.Id,
                (a, r) => new { Answer = a, Response = r })
            .Where(x => x.Response.SurveyId == surveyId)
            .GroupBy(x => x.Answer.SelectedOptionId)
            .Select(g => new { OptionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OptionId, x => x.Count, cancellationToken);
    }
}

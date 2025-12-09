namespace OnlineSurvey.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    ISurveyRepository Surveys { get; }
    IResponseRepository Responses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

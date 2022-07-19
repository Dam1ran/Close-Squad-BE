namespace CS.Application.Persistence.Abstractions;
public interface ITransaction : IAsyncDisposable, IDisposable {
  Guid TransactionId { get; }

  Task CommitAsync(CancellationToken cancellationToken = default);
  Task RollbackAsync(CancellationToken cancellationToken = default);
}

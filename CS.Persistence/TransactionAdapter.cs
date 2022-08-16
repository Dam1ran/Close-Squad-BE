using CS.Application.Persistence.Abstractions;
using CS.Application.Utils;
using Microsoft.EntityFrameworkCore.Storage;

namespace CS.Persistence;
public class TransactionAdapter : ITransaction /* , IAsyncDisposable, IDisposable */ {
  private readonly IDbContextTransaction _dbTransaction;

  public Guid TransactionId => _dbTransaction.TransactionId;

  public TransactionAdapter(IDbContextTransaction dbTransaction) => _dbTransaction = Check.NotNull(dbTransaction, nameof(dbTransaction));

  public Task CommitAsync(CancellationToken cancellationToken = default) => _dbTransaction.CommitAsync(cancellationToken);

  public Task RollbackAsync(CancellationToken cancellationToken = default) => _dbTransaction.RollbackAsync(cancellationToken);

  public ValueTask DisposeAsync() => _dbTransaction.DisposeAsync();

  public void Dispose() => _dbTransaction?.Dispose();
}

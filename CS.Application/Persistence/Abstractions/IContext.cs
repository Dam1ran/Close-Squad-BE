using CS.Core.Entities;

namespace CS.Application.Persistence.Abstractions;
public interface IContext {
  IRepository<User> Users { get; }
  bool HasActiveTransaction { get; }
  ITransaction? CurrentTransaction { get; }

  Task<ITransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default);
  Task CommitTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default);
  Task RollbackTransactionAsync();
  int SaveChanges();
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using System.Reflection;
using CS.Core.Entities;
using CS.Core.Entities.Abstractions;
using CS.Application.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using CS.Application.Utils;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CS.Core.Entities.Auth;

namespace CS.Persistence;
public class Context : IdentityDbContext<User, Role, long>, IContext {

  private TransactionAdapter? _currentTransaction;

  public Context(DbContextOptions<Context> options) : base(options) {
    Announcements = CreateRepository<ServerAnnouncement>();
    Characters = CreateRepository<Character>();
  }
  public IRepository<ServerAnnouncement> Announcements { get; private set; }
  public IRepository<Character> Characters { get; private set; }

  public bool HasActiveTransaction => _currentTransaction != null;

  public ITransaction? CurrentTransaction => _currentTransaction;

  public IRepository<TEntity> CreateRepository<TEntity>() where TEntity: Entity =>
    new Repository<TEntity>(Set<TEntity>());

  int IContext.SaveChanges() => SaveChanges();

  Task<int> IContext.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

  public async Task<ITransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default) {
    if (_currentTransaction != null) {
      return null;
    }

    var transaction = await Database.BeginTransactionAsync(cancellationToken);
    _currentTransaction = new TransactionAdapter(transaction);

    return _currentTransaction;
  }

  public async Task CommitTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default) {
    Check.NotNull(transaction, nameof(transaction));
    if (transaction != _currentTransaction) {
      throw new InvalidOperationException($"Transaction \"{transaction.TransactionId}\" is not current");
    }

    try {
      await SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);
    } catch {
      await RollbackTransactionAsync();
      throw;
    } finally {
      if (_currentTransaction != null) {
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
      }
    }
  }

  public async Task RollbackTransactionAsync() {
    try {
      if (_currentTransaction != null) {
        await _currentTransaction.RollbackAsync();
      }
    } finally {
      if (_currentTransaction != null) {
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
      }
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(GetType())!);
    base.OnModelCreating(modelBuilder);
  }

}

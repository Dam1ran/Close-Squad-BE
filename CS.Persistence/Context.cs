using System.Reflection;
using CS.Application.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using CS.Application.Support.Utils;
using CS.Core.Entities.Auth;
using CS.Core.Entities;

namespace CS.Persistence;
public class Context : DbContext, IContext {

  private TransactionAdapter? _currentTransaction;

  public Context(DbContextOptions<Context> options) : base(options) { }

  public DbSet<CsUser> CsUsers { get; set; } = null!;
  public DbSet<Quadrant> Quadrants { get; set; } = null!;
  public DbSet<ServerAnnouncement> ServerAnnouncements { get; set; } = null!;

  public bool HasActiveTransaction => _currentTransaction != null;

  public ITransaction? CurrentTransaction => _currentTransaction;

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
    modelBuilder.HasDefaultSchema("cs");
    base.OnModelCreating(modelBuilder);
  }

}

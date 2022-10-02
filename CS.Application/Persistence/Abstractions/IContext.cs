using CS.Core.Entities;
using CS.Core.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace CS.Application.Persistence.Abstractions;
public interface IContext {
  public DbSet<CsUser> CsUsers { get; set; }
  public DbSet<Player> Players { get; set; }
  public DbSet<Quadrant> Quadrants { get; set; }
  public DbSet<Character> Characters { get; set; }
  public DbSet<ServerAnnouncement> ServerAnnouncements { get; set; }

  bool HasActiveTransaction { get; }
  ITransaction? CurrentTransaction { get; }

  Task<ITransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default);
  Task CommitTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default);
  Task RollbackTransactionAsync();
  int SaveChanges();
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}

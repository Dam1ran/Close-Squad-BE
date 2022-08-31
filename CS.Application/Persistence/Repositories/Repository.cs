using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Support.Utils;

namespace CS.Application.Persistence.Repositories;
public abstract class Repository : IRepository {
  protected readonly IContext _context;
  public Repository(IContext context) {
    _context = Check.NotNull(context, nameof(context));
  }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
    await _context.SaveChangesAsync(cancellationToken);

}

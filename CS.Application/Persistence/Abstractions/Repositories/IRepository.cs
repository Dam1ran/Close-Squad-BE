namespace CS.Application.Persistence.Abstractions.Repositories;
public interface IRepository {
  Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

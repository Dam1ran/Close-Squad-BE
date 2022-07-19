using CS.Core.Entities.Abstractions;

namespace CS.Application.Persistence.Abstractions;
public interface IRepository<TEntity> where TEntity : Entity {
  TEntity? GetById(int id);
  ValueTask<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
  IReadOnlyList<TEntity> ListAll();
  Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default);
  IReadOnlyList<TEntity> List(ISpecification<TEntity> spec);
  Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
  int Count(ISpecification<TEntity> spec);
  Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
  TEntity First(ISpecification<TEntity> spec);
  Task<TEntity> FirstAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
  TEntity? FirstOrDefault(ISpecification<TEntity> spec);
  Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
  TEntity Single(ISpecification<TEntity> spec);
  Task<TEntity> SingleAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
  TEntity? SingleOrDefault(ISpecification<TEntity> spec);
  Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
  void Add(TEntity entity);
  void AddRange(IEnumerable<TEntity> entities);
  Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
  Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
  void Update(TEntity entity);
  void UpdateRange(IEnumerable<TEntity> entities);
  Task UpdateAsync(TEntity entity);
  Task UpdateRangeAsync(IEnumerable<TEntity> entities);
  void Delete(TEntity entity);
  void DeleteRange(IEnumerable<TEntity> entities);
  Task DeleteAsync(TEntity entity);
  Task DeleteRangeAsync(IEnumerable<TEntity> entities);
}
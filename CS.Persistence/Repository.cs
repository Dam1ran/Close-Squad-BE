using CS.Core.Entities.Abstractions;
using CS.Application.Utils;
using CS.Application.Persistence.Abstractions;
using CS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CS.Persistence;
public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity {
  protected DbSet<TEntity> Set { get; }
  public Repository(DbSet<TEntity> set) {
    Set = Check.NotNull(set, nameof(set));
  }
  public TEntity? GetById(int id) => Set.Find(new object[] { id });

  public ValueTask<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
    Set.FindAsync(new object[] { id }, cancellationToken);

  public IReadOnlyList<TEntity> ListAll() => Set.ToList().AsReadOnly();
  public async Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default) => (await Set.ToListAsync()).AsReadOnly();


  public IReadOnlyList<TEntity> List(ISpecification<TEntity> spec) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).ToList().AsReadOnly();
  }

  public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default) {
    Check.NotNull(spec, nameof(spec));
    return (await Set.ApplySpecification(spec).ToListAsync(cancellationToken)).AsReadOnly();
  }

  public int Count(ISpecification<TEntity> spec) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).Count();
  }

  public Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).CountAsync(cancellationToken);
  }

  public TEntity First(ISpecification<TEntity> spec) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).First();
  }

  public Task<TEntity> FirstAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).FirstAsync(cancellationToken);
  }

  public TEntity? FirstOrDefault(ISpecification<TEntity> spec) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).FirstOrDefault();
  }

  public Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
  }

  public TEntity Single(ISpecification<TEntity> spec) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).Single();
  }

  public Task<TEntity> SingleAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).SingleAsync(cancellationToken);
  }

  public TEntity? SingleOrDefault(ISpecification<TEntity> spec) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).SingleOrDefault();
  }

  public Task<TEntity?> SingleOrDefaultAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default) {
    Check.NotNull(spec, nameof(spec));
    return Set.ApplySpecification(spec).SingleOrDefaultAsync(cancellationToken);
  }

  public void Add(TEntity entity) {
    Check.NotNull(entity, nameof(entity));
    Set.Add(entity);
  }

  public void AddRange(IEnumerable<TEntity> entities) {
    Check.NotNull(entities, nameof(entities));
    Set.AddRange(entities);
  }

  public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
    Check.NotNull(entity, nameof(entity));
    await Set.AddAsync(entity, cancellationToken);
  }

  public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
    Check.NotNull(entities, nameof(entities));
    await Set.AddRangeAsync(entities, cancellationToken);
  }

  public void Update(TEntity entity) {
    Check.NotNull(entity, nameof(entity));
    Set.Update(entity);
  }

  public void UpdateRange(IEnumerable<TEntity> entities) {
    Check.NotNull(entities, nameof(entities));
    Set.UpdateRange(entities);
  }

  public Task UpdateAsync(TEntity entity) {
    Check.NotNull(entity, nameof(entity));
    Set.Update(entity);
    return Task.CompletedTask;
  }

  public Task UpdateRangeAsync(IEnumerable<TEntity> entities) {
    Check.NotNull(entities, nameof(entities));
    Set.UpdateRange(entities);
    return Task.CompletedTask;
  }

  public void Delete(TEntity entity) {
    Check.NotNull(entity, nameof(entity));
    Set.Remove(entity);
  }

  public void DeleteRange(IEnumerable<TEntity> entities) {
    Check.NotNull(entities, nameof(entities));
    Set.RemoveRange(entities);
  }

  public Task DeleteAsync(TEntity entity) {
    Check.NotNull(entity, nameof(entity));
    Set.Remove(entity);
    return Task.CompletedTask;
  }

  public Task DeleteRangeAsync(IEnumerable<TEntity> entities) {
    Check.NotNull(entities, nameof(entities));
    Set.RemoveRange(entities);
    return Task.CompletedTask;
  }
}

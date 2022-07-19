using CS.Core.Entities.Abstractions;

namespace CS.Application.Persistence.Abstractions;
public interface IIncludableSpecification<TEntity, out TProperty>
  : ISpecification<TEntity> where TEntity : Entity {}
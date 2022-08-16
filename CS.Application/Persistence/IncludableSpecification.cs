using CS.Core.Entities.Abstractions;
using CS.Application.Persistence.Abstractions;

namespace CS.Application.Persistence;
public class IncludableSpecification<TEntity, TProperty> :
  Specification<TEntity>, IIncludableSpecification<TEntity, TProperty>
  where TEntity : Entity {
  public IncludableSpecification(ISpecification<TEntity> specification) : base(specification) {}
}

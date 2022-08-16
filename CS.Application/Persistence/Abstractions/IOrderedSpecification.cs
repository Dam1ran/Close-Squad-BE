using System.Linq.Expressions;
using CS.Core.Entities.Abstractions;

namespace CS.Application.Persistence.Abstractions;
public interface IOrderedSpecification<TEntity> : ISpecification<TEntity> where TEntity : Entity {
  IOrderedSpecification<TEntity> ThenBy(Expression<Func<TEntity, object>> expression);
  IOrderedSpecification<TEntity> ThenByDescending(Expression<Func<TEntity, object>> expression);
}

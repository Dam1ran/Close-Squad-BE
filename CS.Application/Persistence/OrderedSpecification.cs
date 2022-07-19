using System.Linq.Expressions;
using CS.Core.Entities.Abstractions;
using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Utils;

namespace CS.Application.Persistence;

public class OrderedSpecification<TEntity> :
  Specification<TEntity>, IOrderedSpecification<TEntity> where TEntity : Entity {
  public OrderedSpecification(ISpecification<TEntity> specification) : base(specification) {}

  public IOrderedSpecification<TEntity> ThenBy(Expression<Func<TEntity, object>> expression) {
    OrderExpressions.Add((expression, OrderType.ThenBy));
    return this;
  }

  public IOrderedSpecification<TEntity> ThenByDescending(Expression<Func<TEntity, object>> expression) {
    OrderExpressions.Add((expression, OrderType.ThenByDescending));
    return this;
  }
}

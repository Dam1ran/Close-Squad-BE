using System.Linq.Expressions;
using CS.Core.Entities.Abstractions;
using CS.Application.Persistence.Utils;
using CS.Application.Persistence.Abstractions;

namespace CS.Application.Persistence;
public abstract class Specification<TEntity> : ISpecification<TEntity> where TEntity: Entity {
  public List<Expression<Func<TEntity, bool>>> WhereExpressions { get; }
      = new List<Expression<Func<TEntity, bool>>>();

  public List<(Expression<Func<TEntity, object>> KeySelector, OrderType OrderType)> OrderExpressions { get; }
      = new List<(Expression<Func<TEntity, object>> KeySelector, OrderType OrderType)>();

  public List<IncludeExpressionInfo> IncludeExpressions { get; }
      = new List<IncludeExpressionInfo>();

  public int? SkipCount { get; private set; }

  public int? TakeCount { get; private set; }

  public bool AsNoTrackingFlag { get; private set; }

  protected Specification() { }

  protected Specification(ISpecification<TEntity> specification) {
    WhereExpressions = specification.WhereExpressions;
    OrderExpressions = specification.OrderExpressions;
    IncludeExpressions = specification.IncludeExpressions;
  }

  public ISpecification<TEntity> Where(Expression<Func<TEntity, bool>> expression) {
    WhereExpressions.Add(expression);
    return this;
  }

  public ISpecification<TEntity> Skip(int count) {
    SkipCount = count;
    return this;
  }

  public ISpecification<TEntity> Take(int count) {
    TakeCount = count;
    return this;
  }

  public ISpecification<TEntity> AsNoTracking() {
    AsNoTrackingFlag = true;
    return this;
  }

  public IIncludableSpecification<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression) {
    IncludeExpressions.Add(new IncludeExpressionInfo(expression, typeof(TEntity), typeof(TProperty)));
    return new IncludableSpecification<TEntity, TProperty>(this);
  }

  public IOrderedSpecification<TEntity> OrderBy(Expression<Func<TEntity, object>> expression) {
    OrderExpressions.Add((expression, OrderType.OrderBy));
    return new OrderedSpecification<TEntity>(this);
  }

  public IOrderedSpecification<TEntity> OrderByDescending(Expression<Func<TEntity, object>> expression) {
    OrderExpressions.Add((expression, OrderType.OrderByDescending));
    return new OrderedSpecification<TEntity>(this);
  }
}

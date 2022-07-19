using System.Linq.Expressions;
using CS.Core.Entities.Abstractions;
using CS.Application.Persistence.Utils;

namespace CS.Application.Persistence.Abstractions;
public interface ISpecification<TEntity> where TEntity : Entity {
  List<Expression<Func<TEntity, bool>>> WhereExpressions { get; }
  List<(Expression<Func<TEntity, object>> KeySelector, OrderType OrderType)> OrderExpressions { get; }
  List<IncludeExpressionInfo> IncludeExpressions { get; }

  int? SkipCount { get; }
  int? TakeCount { get; }

  bool AsNoTrackingFlag { get; }

  ISpecification<TEntity> Where(Expression<Func<TEntity, bool>> expression);
  ISpecification<TEntity> Skip(int count);
  ISpecification<TEntity> Take(int count);
  ISpecification<TEntity> AsNoTracking();
  IIncludableSpecification<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression);
  IOrderedSpecification<TEntity> OrderBy(Expression<Func<TEntity, object>> expression);
  IOrderedSpecification<TEntity> OrderByDescending(Expression<Func<TEntity, object>> expression);
}
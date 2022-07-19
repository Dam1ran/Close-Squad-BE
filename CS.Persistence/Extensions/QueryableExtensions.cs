using System.Linq.Expressions;
using CS.Core.Entities.Abstractions;
using CS.Application.Persistence.Utils;
using CS.Application.Utils;
using CS.Application.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using CS.Application.Persistence;

namespace CS.Persistence.Extensions;

public static class QueryableExtensions {
  public static IQueryable<TEntity> ApplySpecification<TEntity>
    (this IQueryable<TEntity> query, ISpecification<TEntity> specification) where TEntity : Entity {
      Check.NotNull(specification, nameof(specification));

    foreach (var expression in specification.WhereExpressions) {
      query = query.Where(expression);
    }

    foreach (var expression in specification.IncludeExpressions) {
      if (expression is ThenIncludeExpressionInfo thenIncludeExpression) {
        query = query.ThenInclude(thenIncludeExpression);
      } else {
        query = query.Include(expression);
      }
    }

    IOrderedQueryable<TEntity> orderedQuery = Enumerable.Empty<TEntity>().AsQueryable().OrderBy(e => e.Id);
    foreach (var (KeySelector, OrderType) in specification.OrderExpressions) {
      orderedQuery = OrderType switch {
        OrderType.None => orderedQuery,
        OrderType.OrderBy => query.OrderBy(KeySelector),
        OrderType.OrderByDescending => query.OrderByDescending(KeySelector),
        OrderType.ThenBy => orderedQuery.ThenBy(KeySelector),
        OrderType.ThenByDescending => orderedQuery.ThenByDescending(KeySelector),
        _ => throw new ArgumentOutOfRangeException(nameof(OrderType)),
      };
    }

    if (orderedQuery != null) {
      query = orderedQuery;
    }

    if (specification.SkipCount.HasValue) {
      query = query.Skip(specification.SkipCount.Value);
    }

    if (specification.TakeCount.HasValue) {
      query = query.Take(specification.TakeCount.Value);
    }

    if (specification.AsNoTrackingFlag) {
      query = query.AsNoTrackingWithIdentityResolution();
    }

    return query;
  }

  public static IQueryable<T> Include<T>(this IQueryable<T> source, IncludeExpressionInfo info) {
    Check.NotNull(info, nameof(info));

    var queryExpression = Expression.Call(
      typeof(EntityFrameworkQueryableExtensions),
      nameof(EntityFrameworkQueryableExtensions.Include),
      new Type[] { info.EntityType, info.PropertyType },
      source.Expression,
      info.LambdaExpression);

    return source.Provider.CreateQuery<T>(queryExpression);
  }

public static IQueryable<T> ThenInclude<T>(this IQueryable<T> source, ThenIncludeExpressionInfo info) {
  Check.NotNull(info, nameof(info));

  var queryExpression = Expression.Call(
    typeof(EntityFrameworkQueryableExtensions),
    nameof(EntityFrameworkQueryableExtensions.ThenInclude),
    new Type[] { info.EntityType, info.PreviousPropertyType, info.PropertyType },
    source.Expression,
    info.LambdaExpression);

    return source.Provider.CreateQuery<T>(queryExpression);
  }
}

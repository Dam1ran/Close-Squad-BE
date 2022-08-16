using System.Linq.Expressions;
using CS.Application.Utils;

namespace CS.Application.Persistence;
public class IncludeExpressionInfo {
  public LambdaExpression LambdaExpression { get; }
  public Type EntityType { get; }
  public Type PropertyType { get; }

  public IncludeExpressionInfo(
    LambdaExpression expression,
    Type entityType,
    Type propertyType)
  {
    Check.NotNull(expression, nameof(expression));
    Check.NotNull(entityType, nameof(entityType));
    Check.NotNull(propertyType, nameof(propertyType));

    LambdaExpression = expression;
    EntityType = entityType;
    PropertyType = propertyType;
  }
}

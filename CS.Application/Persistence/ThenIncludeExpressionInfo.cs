using System.Linq.Expressions;
using CS.Application.Utils;

namespace CS.Application.Persistence;
public class ThenIncludeExpressionInfo : IncludeExpressionInfo {
  public Type PreviousPropertyType { get; }

  public ThenIncludeExpressionInfo(
    LambdaExpression expression,
    Type entityType,
    Type propertyType,
    Type previousPropertyType)
    : base(expression, entityType, propertyType)
  {
    PreviousPropertyType = Check.NotNull(previousPropertyType, nameof(previousPropertyType));
  }
}


using CS.Application.Exceptions;
using CS.Core.Entities.Abstractions;

namespace CS.Application.Extensions;
public static class EntityExtensions {
  public static TEntity ThrowNotFoundExceptionIfNull<TEntity>(this TEntity entity, string message) where TEntity : Entity
    => entity ?? throw new NotFoundException(message);
}

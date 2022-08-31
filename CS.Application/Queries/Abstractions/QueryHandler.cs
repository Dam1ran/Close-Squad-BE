using CS.Application.Support.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CS.Application.Queries.Abstractions;
public abstract class QueryHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest {
  protected QueryHandler(ILogger logger) {
    _logger = Check.NotNull(logger, nameof(logger));
  }
  protected ILogger _logger { get; private set; }
  public abstract Task<Unit> Handle(TRequest request, CancellationToken cancellationToken);
}

public abstract class QueryHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse> {
  public QueryHandler(ILogger logger) {
    _logger = Check.NotNull(logger, nameof(logger));
  }
  protected ILogger _logger { get; private set; }
  public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
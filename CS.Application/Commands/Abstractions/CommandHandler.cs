using CS.Application.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CS.Application.Commands.Abstractions;
public abstract class CommandHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest {
  protected CommandHandler(ILogger logger) {
    _logger = Check.NotNull(logger, nameof(logger));
  }
  protected ILogger _logger { get; private set; }
  public abstract Task<Unit> Handle(TRequest request, CancellationToken cancellationToken);
}

public abstract class CommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse> {
public CommandHandler(ILogger logger) {
  _logger = Check.NotNull(logger, nameof(logger));
}
  protected ILogger _logger { get; private set; }
  public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
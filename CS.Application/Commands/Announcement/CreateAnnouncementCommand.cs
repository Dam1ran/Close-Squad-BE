using CS.Application.Commands.Abstractions;
using CS.Application.Persistence.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CS.Application.Commands.Announcement;

public class CreateAnnouncementCommand : IRequest {
  public string Message { get; set; } = string.Empty;
}

public class CreateAnnouncementCommandHandler : CommandHandler<CreateAnnouncementCommand> {
  private readonly IContext _context;

  public CreateAnnouncementCommandHandler(
    IContext context,
    ILogger<CreateAnnouncementCommandHandler> logger) : base(logger) {
    _context = Check.NotNull(context, nameof(context));
  }

  public override async Task<Unit> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken) {
    var announcement = new ServerAnnouncement(request.Message);

    await _context.ServerAnnouncements.AddAsync(announcement);
    await _context.SaveChangesAsync();

    return Unit.Value;
  }

}

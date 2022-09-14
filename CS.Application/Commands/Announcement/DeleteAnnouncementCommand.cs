using CS.Application.Commands.Abstractions;
using CS.Application.Persistence.Abstractions;
using CS.Application.Support.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CS.Application.Commands.Announcement;

public class DeleteAnnouncementCommand : IRequest {
  public DeleteAnnouncementCommand(long id) {
    Id = id;
  }

  public long Id { get; set; }
}

public class DeleteAnnouncementCommandHandler : CommandHandler<DeleteAnnouncementCommand> {
  private readonly IContext _context;

  public DeleteAnnouncementCommandHandler(
    IContext context,
    ILogger<DeleteAnnouncementCommandHandler> logger) : base(logger) {
    _context = Check.NotNull(context, nameof(context));
  }

  public override async Task<Unit> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken) {

    var serverAnnouncement = await _context.ServerAnnouncements.SingleOrDefaultAsync(sa => sa.Id == request.Id, cancellationToken);
    if (serverAnnouncement is null) {
      return Unit.Value;
    }

    _context.ServerAnnouncements.Remove(serverAnnouncement);

    await _context.SaveChangesAsync();

    return Unit.Value;

  }

}

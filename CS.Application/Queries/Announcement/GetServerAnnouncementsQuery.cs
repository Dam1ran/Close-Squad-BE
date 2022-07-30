using CS.Application.Models;
using CS.Application.Persistence.Abstractions;
using CS.Application.Queries.Abstractions;
using CS.Application.Specifications;
using CS.Application.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CS.Application.Queries.Announcement;
public class GetServerAnnouncementsQuery : IRequest<IEnumerable<ServerAnnouncementDto>> {}

public class GetServerAnnouncementsQueryHandler : QueryHandler<GetServerAnnouncementsQuery, IEnumerable<ServerAnnouncementDto>> {
  private readonly IContext _context;

  public GetServerAnnouncementsQueryHandler(IContext context,  ILogger<GetServerAnnouncementsQueryHandler> logger) : base(logger) {
    _context = Check.NotNull(context, nameof(context));
  }

  public async override Task<IEnumerable<ServerAnnouncementDto>> Handle(GetServerAnnouncementsQuery request, CancellationToken cancellationToken) {
    var announcements = await _context.Announcements.ListAsync(ServerAnnouncementSpecification.FreshFive(), cancellationToken);

    return announcements.Select(AnnouncementExtensionMethods.MapToDto);
  }
}

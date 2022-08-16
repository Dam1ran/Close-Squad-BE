using CS.Application.Persistence;
using CS.Application.Persistence.Abstractions;
using CS.Core.Entities;

namespace CS.Application.Specifications;

public class ServerAnnouncementSpecification : Specification<ServerAnnouncement> {
  private ServerAnnouncementSpecification() { }
  private ServerAnnouncementSpecification(int id) => Where(q => q.Id == id);

  public static ISpecification<ServerAnnouncement> FreshFive() =>
    new ServerAnnouncementSpecification()
      .OrderByDescending(a => a.CreatedAt)
      .Take(5)
      .AsNoTracking();
}

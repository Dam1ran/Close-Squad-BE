using CS.Core.Entities;

namespace CS.Application.DataTransferObjects;
public class ServerAnnouncementDto {
  public long Id { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public string Message { get; set; } = string.Empty;
}

public static class AnnouncementExtensionMethods {
  public static ServerAnnouncementDto MapToDto(this ServerAnnouncement announcement) {
    return new ServerAnnouncementDto {
      Id = announcement.Id,
      CreatedAt = announcement.CreatedAt,
      Message = announcement.Message
    };
  }
}

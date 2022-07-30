using CS.Core.Entities;

namespace CS.Application.Models;
public class ServerAnnouncementDto {
  public DateTimeOffset CreatedAt { get; set; }
  public string Message { get; set; } = string.Empty;
}

public static class AnnouncementExtensionMethods {
  public static ServerAnnouncementDto MapToDto(this ServerAnnouncement announcement) {
    return new ServerAnnouncementDto {
      CreatedAt = announcement.CreatedAt,
      Message = announcement.Message
    };
  }
}
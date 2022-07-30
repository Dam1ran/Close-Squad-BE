using System.ComponentModel.DataAnnotations;
using CS.Application.Commands.Announcement;

namespace CS.Api.Support.Models;
public class CreateServerAnnouncementViewModel {
  [MinLength(20)]
  [MaxLength(255)]
  public string Message { get; set; } = string.Empty;
  public CreateAnnouncementCommand ToCommand() => new() {
    Message = Message
  };
}
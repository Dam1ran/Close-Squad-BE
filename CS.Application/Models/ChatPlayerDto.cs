using CS.Core.Entities;

namespace CS.Application.Models;
public class ChatPlayerDto {
  public long Id { get; set; }
  public string Nickname { get; set; } = "";
  public string ClanName { get; set; } = "";
  public string ClanIcon { get; set; } = "";

  public static ChatPlayerDto FromPlayer(Player player) =>
    new() {
      Id = player.Id,
      Nickname = player.Nickname.Value,
      ClanName = player.ClanName,
      ClanIcon = player.ClanIcon
    };

}

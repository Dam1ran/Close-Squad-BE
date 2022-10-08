using CS.Core.Entities;

namespace CS.Application.Models;
public class PlayerDto {
  public long Id { get; set; }
  public string Nickname { get; set; } = "";
  public string ClanName { get; set; } = "";
  public string ClanIcon { get; set; } = "";
  public uint? QuadrantIndex { get; set; }

  public static PlayerDto FromPlayer(Player player) =>
    new() {
      Id = player.Id,
      Nickname = player.Nickname.Value,
      ClanName = player.ClanName,
      ClanIcon = player.ClanIcon,
      QuadrantIndex = player.QuadrantIndex
    };

}

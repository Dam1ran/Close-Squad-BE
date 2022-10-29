using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Application.Models;
public class BarShortcutDto {
  public long Id { get; set; }
  public int OrderNumber { get; set; }
  public BarShortcutType Type { get; set; }
  public bool IsActive { get; set; }
  public long UsingId { get; set; }
  public long CharacterId { get; set; }

  public static BarShortcutDto FromBarShortcut(BarShortcut barShortcut) =>
    new() {
      Id = barShortcut.Id,
      Type = barShortcut.Type,
      OrderNumber = barShortcut.OrderNumber,
      IsActive = barShortcut.IsActive,
      UsingId = barShortcut.UsingId,
      CharacterId = barShortcut.CharacterId
    };

}

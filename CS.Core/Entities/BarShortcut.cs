using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;

namespace CS.Core.Entities;
public class BarShortcut: Entity {

  public int OrderNumber { get; set; }
  public BarShortcutType Type { get; set; }
  [NotMapped]
  public bool IsActive { get; set; }
  public long UsingId { get; set; }
  public long CharacterId { get; set; }

}

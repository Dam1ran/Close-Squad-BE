using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Extensions;

namespace CS.Core.Entities;
public class SkillWrapper : Entity {

  public long SkillKeyId { get; set; }

  [NotMapped]
  public DateTimeOffset CoolDown { get; set; }
  [NotMapped]
  public bool IsToggleActivated { get; set; }

  [NotMapped]
  public DateTimeOffset EffectTimeout { get; set; }

  [NotMapped]
  public Skill Skill { get; set; } = null!;

  public long CharacterId { get; set; }
  public bool IsSkillOnCoolDown() => CoolDown.IsInFuture();
}

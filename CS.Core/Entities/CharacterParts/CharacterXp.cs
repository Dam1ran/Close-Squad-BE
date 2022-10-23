
using CS.Core.Entities.Abstractions;
using CS.Core.Support;

namespace CS.Core.Entities;
public partial class Character : Entity, ICsEntity, ICsInstance, ICsAiEntity {
  public double XpPercent => Level_Constants.GetExpPercentFor(Xp, Level);
  public int Level { get; private set; }
  public long Xp { get; private set; }
  public long XpLost { get; set; }
  private readonly object xpLock = new object();

  public long AddXpPercent(double percent) {
    var amount = (long)Math.Ceiling((Level_Constants.GetMaxExpFor(Level) * percent) * 0.01);
    AddXp(amount);
    return amount;
  }

  public void AddXp(long value) {

    lock (xpLock) {
      var sum = Xp + value;
      var level = Level;

      while (sum < 0 || sum > Level_Constants.GetMaxExpFor(level)) {
        if (sum < 0) {
          level--;
          if (level < 0) {
            sum = 0;
            level = 0;
            break;
          }
          sum += Level_Constants.GetMaxExpFor(level);
        } else if (sum > Level_Constants.GetMaxExpFor(level)) {
          sum -= Level_Constants.GetMaxExpFor(level);
          level++;
          if (level > 99) {
            sum = Level_Constants.GetMaxExpFor(99);
            level = 99;
            break;
          }
        }
      }
      if (Level != level) {
        on_level_changed?.Invoke(this, EventArgs.Empty);
      }
      Level = level;
      Xp = sum;

    }

  }
}

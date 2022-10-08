using CS.Core.Support;

namespace CS.Core.Entities.Abstractions;
public abstract class EntityStats: Entity {

  //-----------------HP---------------------------------
  protected uint _hp;
  public uint MaxHP { get; protected set; }
  public uint HP {
    get => _hp;
    set {
      if (value > MaxHP) {
        _hp = MaxHP;
      } else {
        _hp = value;
      }
    }
  }

  //-----------------MP---------------------------------
  protected uint _mp;
  public uint MaxMP { get; protected set; }
  public uint MP {
    get => _mp;
    set {
      if (value > MaxMP) {
        _mp = MaxMP;
      } else {
        _mp = value;
      }
    }
  }

  //-----------------Gender-----------------------------
  protected byte _gender;
  public byte Gender {
    get => _gender;
    protected set {
      if (value < 0) {
        _gender = 0;
      } else if (value > 100) {
        _gender = 100;
      } else {
        _gender = value;
      }
    }
  }

  //-----------------XP---------------------------------
  protected long _xp;
  public long XP { get => _xp; protected set { _xp = value; }  }
  public long AddXp(long value) {

    long sum = 0L;
    Interlocked.Add(ref sum, value);
    if (sum >= Level_Constants.Max_XP_99_lvl) {
      Interlocked.Exchange(ref _xp, Level_Constants.Max_XP_99_lvl);
    } else if (sum < 0L) {
      Interlocked.Exchange(ref _xp, 0L);
    } else if (sum > 0L) {
      Interlocked.Exchange(ref _xp, sum);
    }

    var (level, percent) = Level_Constants.GetLevelAndPercent(_xp);
    Interlocked.Exchange(ref _level, level);

    return _xp;
  }

  //-----------------Lvl--------------------------------
  protected int _level;
  public int Level { get => _level; protected set { _level = value; } }

  public float XP_Percent => Level_Constants.GetLevelAndPercent(_xp).Item2;

}
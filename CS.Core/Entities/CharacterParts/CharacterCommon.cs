
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public partial class Character : Entity, ICsEntity, ICsInstance, ICsAiEntity {
  public Nickname Nickname { get; set; }
  public CsEntityClass Class { get; set; }
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

}

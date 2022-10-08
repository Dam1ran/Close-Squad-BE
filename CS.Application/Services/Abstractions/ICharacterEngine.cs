using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Application.Services.Abstractions;
public interface ICharacterEngine {
  public int TravelTo(TravelDirection travelDirection, Character character, Player player);

}

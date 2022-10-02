using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Core.Services.Interfaces;
public interface IWorldMapService {

  IEnumerable<Tuple<ushort, ushort>> GetQuadrantsIndexesAround(Quadrant quadrant, ushort radius = 1);
  Task<Quadrant> GetStartingQuadrantAsNoTrackingAsync(CharacterRace characterRace, CharacterClass characterClass, CancellationToken cancellationToken);

}

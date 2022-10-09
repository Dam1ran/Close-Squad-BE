using CS.Core.Enums;
using CS.Core.Models;

namespace CS.Core.Services.Interfaces;
public interface IWorldMapService {

  void Init();
  Tuple<ushort, ushort> GetNrOfColsAndRows();
  IEnumerable<uint> GetQuadrantsIndexesAround(uint quadrantIndex, ushort radius = 1);
  uint GetStartingQuadrantIndex(CharacterClass characterClass);
  uint GetArrivingQuadrantIndex(uint quadrantIndex, TravelDirection travelDirection);
  Quadrant GetQuadrantByIndex(uint quadrantIndex);
  Quadrant? GetQuadrantByIndexIfExists(uint quadrantIndex);

}

using CS.Core.Entities;

namespace CS.Core.Services.Interfaces;
public interface IWorldMapService {

  IEnumerable<Tuple<ushort, ushort>> GetQuadrantsIndexesAround(Quadrant quadrant, ushort radius = 1);
  Task<Quadrant> GetStartingQuadrantAsync(CancellationToken cancellationToken);

}

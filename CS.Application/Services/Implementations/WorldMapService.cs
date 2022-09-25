using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Persistence.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CS.Application.Services.Implementations;
public class WorldMapService : IWorldMapService {
  private readonly WorldMapOptions _worldMapOptions;
  private readonly IServiceProvider _serviceProvider;

  public WorldMapService(
    IOptions<WorldMapOptions> worldMapOptions,
    IServiceProvider serviceProvider) {
    _worldMapOptions = Check.NotNull(worldMapOptions?.Value, nameof(worldMapOptions))!;
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
  }

  public IEnumerable<Tuple<ushort, ushort>> GetQuadrantsIndexesAround(Quadrant quadrant, ushort radius = 1) {

    var indexPairs = new List<Tuple<ushort, ushort>>();
    if (radius < 1) {
      indexPairs.Add(new Tuple<ushort, ushort>(quadrant.XIndex, quadrant.YIndex));
      return indexPairs;
    }

    var xMaxIndex = (ushort)(_worldMapOptions.XQuadrants - 1);
    var halfXMaxIndex = (ushort)(Math.Min(radius * 2, xMaxIndex) / 2);

    var yMaxIndex = (ushort)(_worldMapOptions.YQuadrants - 1);
    var halfYMaxIndex = (ushort)(Math.Min(radius * 2, yMaxIndex) / 2);

    for (int x = -halfXMaxIndex; x <= halfXMaxIndex; x++) {
      var xIndex = x + quadrant.XIndex;
      if (xIndex > xMaxIndex) {
        xIndex -= _worldMapOptions.XQuadrants;
      }
      if (xIndex < 0) {
        xIndex += _worldMapOptions.XQuadrants;
      }

      for (int y = -halfYMaxIndex; y <= halfYMaxIndex; y++) {
        var yIndex = y + quadrant.YIndex;
        if (yIndex > yMaxIndex) {
          yIndex -= _worldMapOptions.YQuadrants;
        }
        if (yIndex < 0) {
          yIndex += _worldMapOptions.YQuadrants;
        }

        indexPairs.Add(new Tuple<ushort, ushort>((ushort)xIndex, (ushort)yIndex));
      }

    }

    return indexPairs;

  }

  public async Task<Quadrant> GetStartingQuadrantAsync(CancellationToken cancellationToken) {
    using (var scope = _serviceProvider.CreateScope()) {
      var _context = scope.ServiceProvider.GetRequiredService<IContext>();

      var quadrant =
        await _context.Quadrants
          .SingleOrDefaultAsync(q => q.XIndex == _worldMapOptions.XStartingQuadrant && q.YIndex == _worldMapOptions.YStartingQuadrant, cancellationToken);

      if (quadrant is null) {
        var startingQuadrant = new Quadrant(_worldMapOptions.XStartingQuadrant, _worldMapOptions.YStartingQuadrant);
        await _context.Quadrants.AddAsync(startingQuadrant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return startingQuadrant;
      }

      return quadrant;
    }

  }
}

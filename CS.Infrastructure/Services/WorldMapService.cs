using System.Reflection;
using CS.Application.Options;
using CS.Application.Support.Utils;
using CS.Core.Enums;
using CS.Core.Models;
using CS.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CS.Infrastructure.Services;
public class WorldMapService : IWorldMapService {

  public const int QuadrantSizeInMeters = 100;
  private readonly ILogger<WorldMapService> _logger;
  private WorldMapSettings? _worldMapSettings;

  private List<Quadrant> Quadrants = new List<Quadrant>();

  public WorldMapService(
    ILogger<WorldMapService> logger) {
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public void Init() {
    var path = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!,
        "Files/WorldMap",
        $"WorldMapSettings.json");

    using var settingsReader = new StreamReader(path);
    _worldMapSettings = JsonConvert.DeserializeObject<WorldMapSettings>(settingsReader.ReadToEnd());

    settingsReader.Close();
    settingsReader.Dispose();

    path = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!,
        "Files/WorldMap",
        $"QuadrantsWithData.json");

    using var quadrantsWithDataReader = new StreamReader(path);
    var quadrantsWithData = JsonConvert.DeserializeObject<List<Quadrant>>(quadrantsWithDataReader.ReadToEnd());

    quadrantsWithDataReader.Close();
    quadrantsWithDataReader.Dispose();

    var totalQuadrants = (uint)_worldMapSettings!.NrOfCols * (uint)_worldMapSettings!.NrOfRows;
    for (uint i = 0; i < totalQuadrants; i++) {
      var existingQuadrantForIndex = quadrantsWithData!.SingleOrDefault(q => q.Index == i);

      if (existingQuadrantForIndex is not null) {
        Quadrants.Add(existingQuadrantForIndex);
      } else {
        Quadrants.Add(new Quadrant { Index = i });
      }
    }

    _logger.LogInformation($"-----------------------------------------------------------------------");
    _logger.LogInformation($"Loaded {quadrantsWithData!.Count()} quadrants.");
  }


  public IEnumerable<uint> GetQuadrantsIndexesAround(uint quadrantIndex, ushort radius = 1) {

    if (_worldMapSettings is null) {
      throw new ArgumentNullException(nameof(WorldMapSettings));
    }

    var quadrantIndexes = new List<uint>();
    if (radius < 1) {
      quadrantIndexes.Add(quadrantIndex);
    }

    var xMaxIndex = (ushort)(_worldMapSettings.NrOfCols - 1);
    var halfXMaxIndex = (ushort)(Math.Min(radius * 2, xMaxIndex) / 2);

    var yMaxIndex = (ushort)(_worldMapSettings.NrOfRows - 1);
    var halfYMaxIndex = (ushort)(Math.Min(radius * 2, yMaxIndex) / 2);

    for (int x = -halfXMaxIndex; x <= halfXMaxIndex; x++) {
      var xIndex = x + quadrantIndex.ColIndexRowIndex(_worldMapSettings).Item1;
      if (xIndex > xMaxIndex) {
        xIndex -= _worldMapSettings.NrOfCols;
      }
      if (xIndex < 0) {
        xIndex += _worldMapSettings.NrOfCols;
      }

      for (int y = -halfYMaxIndex; y <= halfYMaxIndex; y++) {
        var yIndex = y + quadrantIndex.ColIndexRowIndex(_worldMapSettings).Item2;
        if (yIndex > yMaxIndex) {
          yIndex -= _worldMapSettings.NrOfRows;
        }
        if (yIndex < 0) {
          yIndex += _worldMapSettings.NrOfRows;
        }

        unchecked {
          quadrantIndexes.Add(ArrayDimensions.GetIndex((ushort)xIndex, (ushort)yIndex, _worldMapSettings));
        }
      }

    }

    return quadrantIndexes;

  }

  public uint GetStartingQuadrantIndex(CharacterClass characterClass) {
    if (_worldMapSettings is null) {
      throw new ArgumentNullException(nameof(WorldMapSettings));
    }

    return (uint) _worldMapSettings.GetType().GetProperty($"{characterClass}StartingIndex")!.GetValue(_worldMapSettings, null)!;
  }

  public uint GetArrivingQuadrantIndex(uint quadrantIndex, TravelDirection travelDirection) {
    if (_worldMapSettings is null) {
      throw new ArgumentNullException(nameof(WorldMapSettings));
    }

    var (colIndex, rowIndex) = quadrantIndex.ColIndexRowIndex(_worldMapSettings);
    var maxColIndex = (ushort)(_worldMapSettings.NrOfCols - 1);
    var maxRowIndex = (ushort)(_worldMapSettings.NrOfRows - 1);

    if (travelDirection == TravelDirection.NE || travelDirection == TravelDirection.E || travelDirection == TravelDirection.SE) {
      if (colIndex >= maxColIndex) {
        colIndex = 0;
      } else {
        colIndex++;
      }
    } else if (travelDirection == TravelDirection.SW || travelDirection == TravelDirection.W || travelDirection == TravelDirection.NW) {
      if (colIndex == 0) {
        colIndex = maxColIndex;
      } else {
        colIndex--;
      }
    }
    if (travelDirection == TravelDirection.NW || travelDirection == TravelDirection.N || travelDirection == TravelDirection.NE) {
      if (rowIndex == 0) {
        rowIndex = maxRowIndex;
      } else {
        rowIndex--;
      }
    } else if (travelDirection == TravelDirection.SE || travelDirection == TravelDirection.S || travelDirection == TravelDirection.SW) {
      if (rowIndex >= maxRowIndex) {
        rowIndex = 0;
      } else {
        rowIndex++;
      }
    }

    return ArrayDimensions.GetIndex(colIndex, rowIndex, _worldMapSettings);

  }

  public Quadrant GetQuadrantByIndex(uint quadrantIndex) =>
    Quadrants.Single(q => q.Index == quadrantIndex);

  public Tuple<ushort, ushort> GetNrOfColsAndRows() {
    if (_worldMapSettings is null) {
      throw new ArgumentNullException(nameof(WorldMapSettings));
    }

    return new(_worldMapSettings.NrOfCols, _worldMapSettings.NrOfRows);
  }

}

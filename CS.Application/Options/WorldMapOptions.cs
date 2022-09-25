using System.ComponentModel.DataAnnotations;

namespace CS.Application.Options;
public class WorldMapOptions {
  public const string WorldMap = "WorldMap";

  [Range(1, 65535)]
  public ushort XQuadrants { get; set; }
  [Range(1, 65535)]
  public ushort YQuadrants { get; set; }
  [Range(1, 65535)]
  public ushort XStartingQuadrant { get; set; }
  [Range(1, 65535)]
  public ushort YStartingQuadrant { get; set; }

}

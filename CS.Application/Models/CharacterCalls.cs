using CS.Core.Enums;

namespace CS.Application.Models;
public class CharacterCall {
  public long CharacterId { get; set; }
}

public class CharacterTravelCall: CharacterCall {
  public TravelDirection TravelDirection { get; set; }
}

public class CharacterScoutCall: CharacterCall {
  public uint QuadrantIndex { get; set; }
}

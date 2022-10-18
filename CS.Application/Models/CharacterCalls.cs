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

public class CharacterMoveCall: CharacterCall {
  public float X { get; set; }
  public float Y { get; set; }

  public bool IsPercent() => X >= 0 && X <= 100 && Y >= 0 && Y <= 100;
}

public class CharacterUseActionCall: CharacterCall {
  public CharacterAction Action { get; set; }
}
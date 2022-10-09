namespace CS.Application.Models;
public class ScoutQuadrantReport {
  public uint QuadrantIndex { get; set; }
  public string Area { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public IEnumerable<CharacterSimpleDto> Characters { get; set; } = new List<CharacterSimpleDto>();
  // public IEnumerable<Character> NonPlayerCharacters { get; set; } = new List<ChatPlayerDto>();
  // public List<Creature> Creatures { get; set; } = new();
  // public ChatPlayerDto? Landlord { get; set; }
  // others...
}

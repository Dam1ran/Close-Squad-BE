namespace CS.Application.Models;

public class AggregatedDataDto {
  public IEnumerable<CharacterDto> ClientCharacters { get; set; } = new List<CharacterDto>();
  public IEnumerable<CharacterSimpleDto> CharactersInActiveQuadrant { get; set; } = new List<CharacterSimpleDto>();

}

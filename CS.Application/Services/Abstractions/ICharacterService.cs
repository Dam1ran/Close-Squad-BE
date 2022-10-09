using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface ICharacterService {

  Task Create(Player player, Nickname characterNickname, CharacterClass characterClass, byte gender, CancellationToken cancellationToken = default);
  Task<IEnumerable<Character>> GetCharactersOf(Player player, CancellationToken cancellationToken = default);
  Character? GetCharacterOf(Player player, long id);
  IEnumerable<Character> GetCharactersInQuadrant(uint quadrantIndex);
  Character? Toggle(Player player, Character character);
  Task<Character?> Update(Player player, Character character, Func<long, Character, Character> updateValueFactory, bool persist = false);

}

using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface ICharacterService {

  Task Create(Player player, Nickname characterNickname, CharacterClass characterClass, byte gender, CancellationToken cancellationToken = default);
  Task<IEnumerable<Character>> GetCharactersOf(Player player, CancellationToken cancellationToken = default);
  IEnumerable<Character> GetAll();
  Character? GetCharacterOf(Player player, long id);
  IEnumerable<Character> GetCharactersInQuadrant(uint quadrantIndex);
  void Toggle(Player player, Character character);
  Task PersistAndClearPlayerCharacters(Player player);
  Task PersistPlayerCharacters(Player player);
  Task Persist(Character character);

}

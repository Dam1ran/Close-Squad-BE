using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface ICharacterService {

  void Init();
  Task Create(Player player, Nickname characterNickname, CsEntityClass characterClass, byte gender, CancellationToken cancellationToken = default);
  Task<IEnumerable<Character>> GetCharactersOf(Player player, CancellationToken cancellationToken = default);
  Task <IEnumerable<BarShortcut>> GetAllCharacterBarShortcutsOfAsync(Player player);
  IEnumerable<Character> GetAll();
  Character? FindCharacterOf(Player player, long id);
  Character? FindCharacterByCsInstanceId(string csInstanceId);
  IEnumerable<Character> GetCharactersInQuadrant(uint quadrantIndex);
  Task PersistAndClearPlayerCharacters(Player player);
  Task PersistPlayerCharacters(Player player);
  Task Persist(Character character);

}

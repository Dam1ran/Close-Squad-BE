using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface ICharacterService {

  Task<IEnumerable<Character>> GetCharactersOf(Player player, CancellationToken cancellationToken = default);
  Task<Character?> Create(Nickname playerNickname, Nickname nickname, CharacterRace characterRace, CharacterClass characterClass, byte gender, CancellationToken cancellationToken = default);
  Character? Toggle(Nickname playerNickname, Nickname characterNickname, CancellationToken cancellationToken = default);
  Quadrant? GetCharacterQuadrant(Nickname playerNickname, Nickname characterNickname, CancellationToken cancellationToken = default);
  Task<Player?> JumpTo(string characterNicknameValue, Player player, CancellationToken cancellationToken = default);

}

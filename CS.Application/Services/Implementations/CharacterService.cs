using System.Collections.Concurrent;
using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.Services.Interfaces;
using CS.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace CS.Application.Services.Implementations;
public class CharacterService : ICharacterService {

  private readonly IServiceProvider _serviceProvider;
  private readonly IWorldMapService _worldMapService;
  private readonly IPlayerService _playerService;
  private readonly ConcurrentDictionary<long, ConcurrentDictionary<long, Character>> Characters = new();

  public static readonly int MaxNumberOfCharacters = Enum.GetNames(typeof(CharacterClass)).Length;


  public CharacterService(IServiceProvider serviceProvider, IWorldMapService worldMapService, IPlayerService playerService) {
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
  }

    public async Task Create(Player player, Nickname characterNickname, CharacterClass characterClass, byte gender, CancellationToken cancellationToken = default) {

    using var scope = _serviceProvider.CreateScope();
    var _context = scope.ServiceProvider.GetRequiredService<IContext>();

    var newCharacter = new Character(
      characterNickname,
      characterClass,
      _worldMapService.GetStartingQuadrantIndex(characterClass),
      gender);

    player.Characters.Add(newCharacter);
    _context.Players.Update(player);
    await _context.SaveChangesAsync(cancellationToken);

    if (!Characters.TryGetValue(player.Id, out var characters)) {
      characters = new ();
    }

    foreach (var character in player.Characters) {
      characters.TryAdd(character.Id, character);
    }

    Characters.TryAdd(player.Id, characters);

  }

  public async Task<IEnumerable<Character>> GetCharactersOf(Player player, CancellationToken cancellationToken = default) {

    if (Characters.TryGetValue(player.Id, out var characters)) {
      return characters.Values;
    }

    using (var scope = _serviceProvider.CreateScope()) {
      var _playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
      var repoCharacters = await _playerRepo.GetPlayerCharactersAsync(player.Id, cancellationToken);
      if (repoCharacters.Count() == 0) {
        return Enumerable.Empty<Character>();
      }

      var dict = new ConcurrentDictionary<long, Character>();
      foreach (var character in repoCharacters) {
        dict.TryAdd(character.Id, character);
      }

      Characters.TryAdd(player.Id, dict);

      return dict.Values;

    }

  }

  public Character? GetCharacterOf(Player player, long id) {
    if (Characters.TryGetValue(player.Id, out var _characters) &&
        _characters.TryGetValue(id, out var _character))
    {
      return _character;
    }

    return null;
  }



  public Character? Toggle(Player player, Character character) {
    if (!Characters.TryGetValue(player.Id, out var characters)) {
      return null;
    }

    if (character.CharacterStatus == CharacterStatus.Engaged) {
      return character;
    }

    return characters.AddOrUpdate(
      character.Id,
      character,
      (key, existing) => {
        existing.CharacterStatus
          = character.CharacterStatus != CharacterStatus.Astray
          ? CharacterStatus.Astray
          : character.HP > 0
          ? CharacterStatus.Awake
          : CharacterStatus.Dead;

        return existing;
      });

  }

  public Character? Update(Player player, Character character, Func<long, Character, Character> updateValueFactory) {
    if (!Characters.TryGetValue(player.Id, out var characters)) {
      return null;
    }

    return characters.AddOrUpdate(character.Id, character, updateValueFactory);
  }
}

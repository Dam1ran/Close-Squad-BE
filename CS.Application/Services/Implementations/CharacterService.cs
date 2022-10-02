using System.Collections.Concurrent;
using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.Services.Interfaces;
using CS.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CS.Application.Services.Implementations;
public class CharacterService : ICharacterService {

  private readonly IServiceProvider _serviceProvider;
  private readonly IWorldMapService _worldMapService;
  private readonly IPlayerService _playerService;
  private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Character>> Characters = new();

  public readonly int MaxNumberOfCharacters = Enum.GetNames(typeof(CharacterClass)).Length;


  public CharacterService(IServiceProvider serviceProvider, IWorldMapService worldMapService, IPlayerService playerService) {
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
  }

  public async Task<IEnumerable<Character>> GetCharactersOf(Player player, CancellationToken cancellationToken = default) {

    if (Characters.TryGetValue(player.Nickname.ValueLowerCase, out ConcurrentDictionary<string, Character>? characters)) {
      return characters.Values;
    }

    using (var scope = _serviceProvider.CreateScope()) {
      var _playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
      var repoCharacters = await _playerRepo.GetPlayerCharactersWithQuadrantAsync(player, cancellationToken);
      if (repoCharacters.Count() == 0) {
        return Enumerable.Empty<Character>();
      }

      var dict = new ConcurrentDictionary<string, Character>();
      foreach (var character in repoCharacters) {
        dict.TryAdd(character.Nickname.ValueLowerCase, character);
      }

      Characters.TryAdd(player.Nickname.ValueLowerCase, dict);

      return dict.Values;

    }

  }

  public async Task<Character?> Create(Nickname playerNickname, Nickname nickname, CharacterRace characterRace, CharacterClass characterClass, byte gender, CancellationToken cancellationToken = default) {

    using (var scope = _serviceProvider.CreateScope()) {
      var _playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
      var _context = scope.ServiceProvider.GetRequiredService<IContext>();

      var existingCharacter = await _context.Characters
        .SingleOrDefaultAsync(c => c.Nickname.ValueLowerCase == nickname.ValueLowerCase, cancellationToken);

      if (existingCharacter is not null) {
        return null;
      }

      var startingQuadrant = await _worldMapService.GetStartingQuadrantAsNoTrackingAsync(characterRace, characterClass, cancellationToken);

      var player = await _playerRepo.FindByNicknameWithCharactersAsync(playerNickname, cancellationToken);
      if (player is null) {
        player = new Player(playerNickname, startingQuadrant);
      }

      if (player.Characters.Count() >= MaxNumberOfCharacters) {
        return null;
      }

      player.Quadrant = startingQuadrant;
      var newCharacter = new Character(startingQuadrant, nickname, characterRace, characterClass, gender);
      player.Characters.Add(newCharacter);
      _context.Players.Update(player);
      await _context.SaveChangesAsync(cancellationToken);

      await _playerService.UpdatePlayerQuadrant(playerNickname, startingQuadrant, cancellationToken);

      if (!Characters.TryGetValue(playerNickname.ValueLowerCase, out ConcurrentDictionary<string, Character>? characters)) {
        characters = new ConcurrentDictionary<string, Character>();
      }

      foreach (var character in player.Characters) {
        characters.TryAdd(character.Nickname.ValueLowerCase, character);
      }

      Characters.TryAdd(playerNickname.ValueLowerCase, characters);

      return newCharacter;

    }

  }

  public Character? Toggle(Nickname playerNickname, Nickname characterNickname, CancellationToken cancellationToken = default) {

    if (Characters.TryGetValue(playerNickname.ValueLowerCase, out ConcurrentDictionary<string, Character>? characters)) {
      if (characters.TryGetValue(characterNickname.ValueLowerCase, out Character? character)) {
        return characters.AddOrUpdate(characterNickname.ValueLowerCase, character, (key, existing) =>
        {
          existing.IsAwake = !existing.IsAwake;
          return existing;
        });
      }
    }

    return null;
  }

    public async Task<Player?> JumpTo(string characterNicknameValue, Player player, CancellationToken cancellationToken = default) {
    if (Nickname.IsWrongNickname(characterNicknameValue, out Nickname? characterNickname)) {
      return null;
    }

    if (Characters.TryGetValue(player.Nickname.ValueLowerCase, out ConcurrentDictionary<string, Character>? characters)) {
      if (characters.TryGetValue(characterNickname!.ValueLowerCase, out Character? character)) {
        return await _playerService.UpdatePlayerQuadrant(player.Nickname, character.Quadrant, cancellationToken);
      }
    }

    return null;

  }

  public Quadrant? GetCharacterQuadrant(Nickname playerNickname, Nickname characterNickname, CancellationToken cancellationToken = default) {

    if (Characters.TryGetValue(playerNickname.ValueLowerCase, out ConcurrentDictionary<string, Character>? characters)) {
      if (characters.TryGetValue(characterNickname.ValueLowerCase, out Character? character)) {
        return character.Quadrant;
      }
    }

    return null;

  }

}

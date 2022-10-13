using System.Reflection;
using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.Exceptions;
using CS.Core.Models;
using CS.Core.Services.Interfaces;
using CS.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CS.Application.Services.Implementations;
public class CharacterService : ICharacterService {

  private readonly IServiceProvider _serviceProvider;
  private readonly IWorldMapService _worldMapService;
  private readonly IPlayerService _playerService;
  private readonly ITickService _tickService;
  private readonly List<Tuple<long, Character>> Characters = new();

  public static readonly int MaxNumberOfCharacters = Enum.GetNames(typeof(CharacterClass)).Length;
  private const int CharacterBufferClearIntervalSeconds = 60;

  private readonly object listLock = new object();
  private CharacterBaseStatsHelper characterBaseStatsHelper = new();

  public CharacterService(
    IServiceProvider serviceProvider,
    IWorldMapService worldMapService,
    IPlayerService playerService,
    ITickService tickService) {
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _tickService = Check.NotNull(tickService, nameof(tickService));

    _tickService.on_60s_tick += ClearLoggedOutPlayersAndCharacters;
  }

  public void Init() {

    var characterBaseStatsPath = Path.Combine(Path.GetDirectoryName(
      Assembly.GetExecutingAssembly().Location)!,
      "Files/Character",
      $"CharacterBaseStats.json");

    using var characterBaseStatsReader = new StreamReader(characterBaseStatsPath);
    characterBaseStatsHelper.CharacterBaseStats
      = JsonConvert.DeserializeObject<CharacterBaseStats>(characterBaseStatsReader.ReadToEnd())
      ?? throw new NotFoundException("CharacterBaseStats.json could not be loaded");

    characterBaseStatsReader.Close();
    characterBaseStatsReader.Dispose();

    var classStatsModifiersPath = Path.Combine(Path.GetDirectoryName(
      Assembly.GetExecutingAssembly().Location)!,
      "Files/Character",
      $"ClassStatsModifiers.json");

    using var classStatsModifiersReader = new StreamReader(classStatsModifiersPath);
    characterBaseStatsHelper.ClassStatsModifiers
      = JsonConvert.DeserializeObject<List<ClassStatsModifiers>>(classStatsModifiersReader.ReadToEnd())
      ?? throw new NotFoundException("ClassStatsModifiers.json could not be loaded");

    classStatsModifiersReader.Close();
    classStatsModifiersReader.Dispose();

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

  }

  public async Task<IEnumerable<Character>> GetCharactersOf(Player player, CancellationToken cancellationToken = default) {

    var characters = Characters.Where(cl => cl.Item1 == player.Id);
    if (characters.Any()) {
      return characters.Select(cl => cl.Item2);
    }

    using var scope = _serviceProvider.CreateScope();
    var _playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();

    var repoCharacters = await _playerRepo.GetPlayerCharactersAsNoTrackingAsync(player.Id, cancellationToken);
    if (repoCharacters.Any()) {
      lock (listLock) {

        foreach (var character in repoCharacters) {
          if (Characters.SingleOrDefault(cl => cl.Item2.Id == character.Id) is null) {
            characterBaseStatsHelper.AssignBaseStats(character);
            Characters.Add(new (player.Id, character));
          }
        }

      }

      return repoCharacters;

    }

    return Enumerable.Empty<Character>();

  }

  public IEnumerable<Character> GetCharactersInQuadrant(uint quadrantIndex) =>
    Characters.Where(cl => cl.Item2.QuadrantIndex == quadrantIndex).Select(cl => cl.Item2);

  public Character? GetCharacterOf(Player player, long id) =>
    Characters.SingleOrDefault(cl => cl.Item1 == player.Id && cl.Item2.Id == id)?.Item2;

  public void Toggle(Player player, Character character) {

    if (character.CharacterStatus == CharacterStatus.Engaged) {
      return;
    }

    character.CharacterStatus
      = character.CharacterStatus != CharacterStatus.Astray
      ? CharacterStatus.Astray
      : character.HpStat.Current > 0
      ? CharacterStatus.Awake
      : CharacterStatus.Dead;

  }

  public async Task Persist(Character character) {
    using var scope = _serviceProvider.CreateScope();
    var _context = scope.ServiceProvider.GetRequiredService<IContext>();

    _context.Characters.Update(character);
    await _context.SaveChangesAsync();
  }

  private void ClearLoggedOutPlayersAndCharacters(object? sender, EventArgs e) {
    var players = _playerService.GetPlayers();
    foreach (var player in players) {
      if (player.LogoutAt.HasValue && player.LogoutAt.Value.AddSeconds(CharacterBufferClearIntervalSeconds) < DateTimeOffset.UtcNow) {
        // if this will be called quicker than the time it takes to do the actual update
        // then consider a checking mechanism for tasks already running
        // for now it runs only by one thread each minute
        _ = PersistAndClearPlayerCharacters(player);
      }
    }
  }

  public async Task PersistAndClearPlayerCharacters(Player player) {

    await PersistPlayerCharacters(player);

    Characters.RemoveAll(cl => cl.Item1 == player.Id);
    _playerService.ClearPlayer(player);

  }

  public async Task PersistPlayerCharacters(Player player) {
    if (!Characters.Any(cl => cl.Item1 == player.Id)) {
      return;
    }

    using var scope = _serviceProvider.CreateScope();
    var _context = scope.ServiceProvider.GetRequiredService<IContext>();

    _context.Characters.UpdateRange(await GetCharactersOf(player));
    await _context.SaveChangesAsync();
  }

  public IEnumerable<Character> GetAll() => Characters.Select(cl => cl.Item2);

}


public class CharacterBaseStatsHelper {
  public CharacterBaseStats CharacterBaseStats { get; set; } = new CharacterBaseStats();
  public List<ClassStatsModifiers> ClassStatsModifiers { get; set; } = new ();

  public void AssignBaseStats(Character character) =>
    CharacterBaseStats
    .AssignBaseStats(
      character,
      ClassStatsModifiers.Where(csm => csm.CharacterClass == character.CharacterClass).First()
    );

}

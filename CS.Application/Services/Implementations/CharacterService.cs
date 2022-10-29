using System.Reflection;
using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.Services.Interfaces;
using CS.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace CS.Application.Services.Implementations;
public class CharacterService : ICharacterService {

  private readonly IServiceProvider _serviceProvider;
  private readonly IWorldMapService _worldMapService;
  private readonly ISkillService _skillService;
  private readonly IPlayerService _playerService;
  private readonly ITickService _tickService;
  private readonly IHubSender _hubSender;
  private readonly List<Tuple<long, Character>> Characters = new();

  public static readonly int MaxNumberOfCharacters = Enum.GetNames(typeof(CsEntityClass)).Length;
  private const int CharacterBufferClearIntervalSeconds = 60;

  private readonly object listLock = new object();
  private CharacterStatsHelper characterStatsHelper = new();

  public CharacterService(
    IServiceProvider serviceProvider,
    IWorldMapService worldMapService,
    ISkillService skillService,
    IPlayerService playerService,
    IHubSender hubSender,
    ITickService tickService) {
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _skillService = Check.NotNull(skillService, nameof(skillService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _hubSender = Check.NotNull(hubSender, nameof(hubSender));
    _tickService = Check.NotNull(tickService, nameof(tickService));

    _tickService.on_60s_tick += ClearLoggedOutPlayersAndCharacters;
  }
    public void Init() {
      characterStatsHelper.Init();
    }

    public async Task Create(Player player, Nickname characterNickname, CsEntityClass characterClass, byte gender, CancellationToken cancellationToken = default) {

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

    var repoCharacters = await _playerRepo.GetPlayerCharactersWithShortcutsSkillWrappersAsNoTrackingAsync(player.Id, cancellationToken);
    if (repoCharacters.Any()) {
      lock (listLock) {

        var rangeHolder = new List<Tuple<long, Character>>();
        foreach (var character in repoCharacters) {
          if (Characters.SingleOrDefault(cl => cl.Item2.Id == character.Id) is not null) {
            continue;
          }

          characterStatsHelper.ConnectHandlersAndInit(character);
          characterStatsHelper.RecalculateStats(character);

          character.on_zero_hp += (sender, args) => {
            if (sender is Character characterToPersist) {
              _ = Task.Run(() => PersistAsync(characterToPersist)).ConfigureAwait(false);
            }
          };

          character.on_damage_incurred += (sender, args) => {
            if (sender is null || args is null) {
              return;
            }

            _hubSender.EnqueueSystemMessage(
              player.Nickname,
              $"{((ICsEntity)sender).Nickname} incurred {(int)Math.Abs(args.Damage)} damage to {args.CsEntity.Nickname}.");
          };

          character.on_damage_received += (sender, args) => {
            if (sender is null || args is null) {
              return;
            }

            if (sender is Character characterReceiver) {
              var playerReceiverNickname = _playerService.GetPlayerNickname(characterReceiver.PlayerId);
              if (string.IsNullOrWhiteSpace(playerReceiverNickname.Value)) {
                return;
              }

              _hubSender.EnqueueSystemMessage(
                playerReceiverNickname,
                $"{characterReceiver.Nickname} received {(int)Math.Abs(args.Damage)} damage from {args.CsEntity.Nickname}.");
            }

          };

          rangeHolder.Add(new (player.Id, character));
        }

        _skillService.AssignSkills(rangeHolder.Select(rh => rh.Item2));
        _skillService.CleanUpSkills(rangeHolder.Select(rh => rh.Item2));
        Characters.AddRange(rangeHolder);
        rangeHolder.Clear();

      }

      return repoCharacters;

    }

    return Enumerable.Empty<Character>();

  }

  public IEnumerable<Character> GetCharactersInQuadrant(uint quadrantIndex) =>
    Characters.Where(cl => cl.Item2.QuadrantIndex == quadrantIndex).Select(cl => cl.Item2);

  public Character? FindCharacterOf(Player player, long id) =>
    Characters.SingleOrDefault(cl => cl.Item1 == player.Id && cl.Item2.Id == id)?.Item2;

  public async Task PersistAsync(Character character) {
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

    foreach(var csEntity in await GetCharactersOf(player)) {
      csEntity.ClearAllEventHandlers();
    }

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

  public async Task<IEnumerable<BarShortcut>> GetAllCharacterBarShortcutsOfAsync(Player player) =>
    (await GetCharactersOf(player)).SelectMany(c => c.BarShortcuts);

  public Character? FindCharacterByCsInstanceId(string csInstanceId) =>
    Characters.SingleOrDefault(cl => cl.Item2.CsInstanceId == csInstanceId)?.Item2;

  public async Task AssignBarShortcutAsync(Character character, BarShortcutType barShortcutType, long usingId, int shortcutIndex) {
    var shortcut = character.BarShortcuts.SingleOrDefault(bs => bs.OrderNumber == shortcutIndex);
    if (shortcut is null) {
      shortcut = new (barShortcutType, shortcutIndex, usingId);
      character.BarShortcuts.Add(shortcut);
    }
    else
    {
      shortcut.Type = barShortcutType;
      shortcut.UsingId = usingId;
    }

    await PersistAsync(character);

  }

  public async Task RemoveBarShortcutAsync(Character character, int shortcutIndex) {
    var shortcut = character.BarShortcuts.SingleOrDefault(bs => bs.OrderNumber == shortcutIndex);
    if (shortcut is null) {
      return;
    }

    character.BarShortcuts.Remove(shortcut);

    using var scope = _serviceProvider.CreateScope();
    var _context = scope.ServiceProvider.GetRequiredService<IContext>();

    _context.BarShortcuts.Remove(shortcut);
    await _context.SaveChangesAsync();

  }
  public IEnumerable<ICsEntity> GetSkillAffectedTargets(Character character, SkillWrapper skillWrapper) {
    var targets = new List<ICsEntity>();

    if (skillWrapper.Skill.TargetType == SkillTargetType.Self) {
      targets.Add(character);
    }

    if (skillWrapper.Skill.TargetType == SkillTargetType.Target && character.Target is not null) {
      if (skillWrapper.Skill.Kind == SkillKindType.All) {
        targets.Add(character.Target);
      }

      if (character.Target.Status == CsEntityStatus.Astray ||
          character.Target.Status == CsEntityStatus.Traveling ||
          character.Target.Status == CsEntityStatus.Dead ||
          character.QuadrantIndex != character.Target.QuadrantIndex
      ) {
        return targets;
      }

      var isTargetFriendly = IsICsEntityFriendlyTo(character.Target, character);
      var isInRange = IsPositionInRangeOfPosition(character.Target.Position, character.Position, skillWrapper.Skill.CastRange);

      if (skillWrapper.Skill.Kind == SkillKindType.Enemy && !isTargetFriendly && isInRange) {
        targets.Add(character.Target);
      }
      if (skillWrapper.Skill.Kind == SkillKindType.Friendly && isTargetFriendly && isInRange) {
        targets.Add(character.Target);
      }
    }

    if (skillWrapper.Skill.TargetType == SkillTargetType.SelfRadius) {
      targets.Add(character);

      // add targets around character if any in radius

      // var charactersInSameQuadrant = GetCharactersInQuadrant(character.QuadrantIndex)
      //   .Where(c => c.Status != CsEntityStatus.Astray && c.Status != CsEntityStatus.Traveling && c.Status != CsEntityStatus.Dead);
    }

    if (skillWrapper.Skill.TargetType == SkillTargetType.TargetRadius && character.Target is not null) {
      targets.Add(character.Target);
      // add targets around characters target if any in radius
    }


    return targets;

  }

  private bool IsICsEntityFriendlyTo(ICsEntity csEntity, Character character) {
    var isFriendly = false;

    // if (csEntity is Actor actor) {
      // true
    // }
    // if (csEntity is Creature creature) {
      // false
    // }
    if (csEntity is Character targetCharacter) {
      isFriendly = character.PlayerId == targetCharacter.PlayerId;
      // character.PartyId == targetCharacter.PartyId ||
      // character.ClanId == targetCharacter.ClanId ||
      // character.IsFriend(targetCharacter)
    }

    return isFriendly;
    /*
      same quadrant and alive and
      own character or
      character of party player or
      character of clan player or
      character of friend player or
      actor
    */
  }

  private bool IsPositionInRangeOfPosition(Position targetPosition, Position position, double? range) {
    if (!range.HasValue || range.Value <= 0) {
      return false;
    }

    return targetPosition.GetDistance(position) < range;
  }

}

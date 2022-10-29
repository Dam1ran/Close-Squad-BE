using System.Text.RegularExpressions;
using CS.Api.Communications.Models;
using CS.Application.Enums;
using CS.Application.Models;
using CS.Application.Models.Dialog;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Communications;
public partial class MainHub : Hub<ITypedHubClient> {

  private readonly ILogger<MainHub> _logger;
  private readonly ICacheService _cacheService;
  private readonly IPlayerService _playerService;
  private readonly ICharacterService _characterService;
  private readonly ICharacterEngine _characterEngine;
  private readonly IWorldMapService _worldMapService;
  private readonly IHubService _hubService;

  public MainHub(
    ILogger<MainHub> logger,
    ICacheService cacheService,
    IPlayerService playerService,
    ICharacterService characterService,
    ICharacterEngine characterEngine,
    IWorldMapService worldMapService,
    IHubService hubService)
  {
    _logger = Check.NotNull(logger, nameof(logger));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
    _characterEngine = Check.NotNull(characterEngine, nameof(characterEngine));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _hubService = Check.NotNull(hubService, nameof(hubService));
  }

  public async Task SendChatMessage(ChatMessage chatMessage) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    if (await IsBanned()) {
      await SendSystemChatMessage("User banned.");
      return;
    }

    if (chatMessage.Text.Length > 255) {
      chatMessage.Text = chatMessage.Text.Substring(0, 255);
    }

    chatMessage.Text = Regex.Replace(chatMessage.Text, @"^(\/[n|w|p|c|s]) ", "").Trim();

    if (!Enum.IsDefined(typeof(ChatMessageType), chatMessage.Type) || string.IsNullOrWhiteSpace(chatMessage.Text)) {
      return;
    }

    var isShout = chatMessage.Type == ChatMessageType.Shout;
    var playerAllowedToShout = true; // TODO
    if (isShout && !playerAllowedToShout) {
      await SendSystemChatMessage("Shout not available at this time.");
      return;
    }

    var playerHasBanChat = false; // TODO
    if (playerHasBanChat) {
      await SendSystemChatMessage($"Chat is silenced for {12}m.");
      return;
    }

    var cachedMessage = await _cacheService
      .GetAsync<CachedChatMessage>(
        isShout ? CacheGroupKeyConstants.UserShoutThrottle : CacheGroupKeyConstants.UserChatThrottle,
        Context.UserIdentifier!);

    if (
      cachedMessage is not null &&
      (cachedMessage.Text.Equals(chatMessage.Text) ||
      cachedMessage.SentAt.AddSeconds(1) > DateTimeOffset.UtcNow)) {
      return;
    }

    if (isShout && cachedMessage is not null) {
      await SendSystemChatMessage($"Shout on cool down for {120 - (int)(DateTimeOffset.UtcNow - cachedMessage.SentAt).TotalSeconds}s.");
      return;
    }

    await _cacheService
      .SetAsync(
        isShout ? CacheGroupKeyConstants.UserShoutThrottle : CacheGroupKeyConstants.UserChatThrottle,
        Context.UserIdentifier!,
        new CachedChatMessage() { Text = chatMessage.Text, SentAt = DateTimeOffset.UtcNow, Type = chatMessage.Type },
        absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(isShout ? 2 : 1));

    switch (chatMessage.Type) {
      case ChatMessageType.Nearby: {
        await ProcessNearbyMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Whisper: {
        await ProcessWhisperMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Party: {
        await ProcessPartyMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Clan: {
        await ProcessClanMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Shout: {
        await ProcessShoutMessage(currentPlayer, chatMessage);
        return;
      }
      default: return;
    }
  }

  // public async Task SendChatCommand(ChatCommand chatCommand) {
  //   Debug.WriteLine(chatCommand.Text);
  // }

  // [Authorize(Policy = Api_Constants.AdministrationPolicy)]
  // [Authorize(Policy = Api_Constants.ManagementPolicy)]
  // [Authorize(Policy = Api_Constants.GameMasterPolicy)]
  // public async Task SendBanPlayer(Player player) {
  //   Debug.WriteLine(player.Nickname);
  // }

  public override async Task OnConnectedAsync() {

    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      await Clients.Caller.SendServerDialog(new() {
        Message = "Create a character to use chat.",
        CanBeClosed = true,
        DialogType = DialogType.Warning,
        Title = "No character..."
      });
    } else {
      if (!string.IsNullOrWhiteSpace(currentPlayer.ConnectionId)) {
        await Clients.Client(currentPlayer.ConnectionId).Disconnect();
      }
      currentPlayer = _playerService.ClearLogoutTimeAndSetId(currentPlayer, Context.ConnectionId);
      await _hubService.SetCurrentPlayer(currentPlayer);
      await SendPlayerCharacters(currentPlayer);

      var characterBarShortcuts = await _characterService.GetAllCharacterBarShortcutsOfAsync(currentPlayer);
      await Clients.Caller.SetBarShortcuts(characterBarShortcuts.Select(BarShortcutDto.FromBarShortcut));
    }

    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception) {

    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is not null) {
      if (currentPlayer.QuadrantIndex.HasValue) {
        await _hubService.SendAllUpdateQuadrantPlayerList(currentPlayer);
        currentPlayer = _playerService.UpdatePlayerQuadrant(currentPlayer, null);
      }

      _playerService.SetLogoutTime(currentPlayer);
    }

    await SendSystemChatMessage("Disconnected.");
    await base.OnDisconnectedAsync(exception);
  }

  private async Task<(Player?, Character?)> GetPlayerAndCharacter(CharacterCall characterCall) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is not null) {
      var character = _characterService.FindCharacterOf(currentPlayer, characterCall.CharacterId);
      return (currentPlayer, character);
    }
    return (null, null);
  }

  public async Task PlayerJumpTo(CharacterCall characterCall) {

    var (currentPlayer, character) = await GetPlayerAndCharacter(characterCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    if (currentPlayer.QuadrantIndex == character!.QuadrantIndex) {
      return;
    }

    await _hubService.SendAllUpdateQuadrantPlayerList(currentPlayer);

    var jumpedPlayer = _playerService.UpdatePlayerQuadrant(currentPlayer, character.QuadrantIndex);
    await _hubService.SetCurrentPlayer(jumpedPlayer);
    await _hubService.SendAllUpdateQuadrantPlayerList(jumpedPlayer, true);
  }

  public async Task PlayerLeaveQuadrant() {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    await _hubService.SendAllUpdateQuadrantPlayerList(currentPlayer);

    await Clients.Caller.SetNearbyGroup(Enumerable.Empty<ChatPlayerDto>());
    var player = _playerService.UpdatePlayerQuadrant(currentPlayer, null);
    await _hubService.SetCurrentPlayer(player);
  }

  public async Task CharacterToggle(CharacterCall characterCall) {
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    character.Toggle();

    var anyOwnPresentCharactersInSameQuadrant = (await _characterService
      .GetCharactersOf(currentPlayer))
      .Any(c => c.QuadrantIndex == character.QuadrantIndex &&
          c.Status != CsEntityStatus.Astray && c.Status != CsEntityStatus.Traveling);

    if (currentPlayer?.QuadrantIndex == character.QuadrantIndex && !anyOwnPresentCharactersInSameQuadrant) {
      await PlayerLeaveQuadrant();
    }

    await Clients.Caller.UpdateCharacter(new { Id = character.Id, CharacterStatus = character.Status });

  }

  public async Task CharacterTravelTo(CharacterTravelCall characterTravelCall) {
    if (!Enum.IsDefined(typeof(TravelDirection), characterTravelCall.TravelDirection)) {
      return;
    }

    var (currentPlayer, character) = await GetPlayerAndCharacter(characterTravelCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    // TODO: check for quadrant requirements...
    character.SetTraveling();

    _ = Clients.Caller.UpdateCharacter(new { Id = character.Id, CharacterStatus = character.Status });

    var secondsToTravel = _characterEngine.TravelTo(characterTravelCall.TravelDirection, character, currentPlayer);

    _ = SendSystemChatMessage($"{character.Nickname} will arrive in {secondsToTravel} seconds.");

  }
  public async Task CharacterMove(CharacterMoveCall characterMove) {
    if (!characterMove.IsPercent()) {
      return;
    }

    var (currentPlayer, character) = await GetPlayerAndCharacter(characterMove);
    if (currentPlayer is null || character is null || !character.IsAlive()) {
      return;
    }

    character.MoveTo(characterMove.X, characterMove.Y);

  }

  public async Task ScoutQuadrant(CharacterScoutCall characterScoutCall) {
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterScoutCall);
    if (currentPlayer is null || character is null  /* || no consumable */) {
      return;
    }
    
    // character.Inventory.Decrement(consumable)

    var quadrant = _worldMapService.GetQuadrantByIndexIfExists(characterScoutCall.QuadrantIndex);
    if (quadrant is null) {
      return;
    }

    var report = new ScoutQuadrantReport() {
      QuadrantIndex = quadrant.Index,
      Area = quadrant.Area,
      Name = quadrant.Name,
      Characters = _characterService
        .GetCharactersInQuadrant(quadrant.Index)
        .Where(c => c.Class != CsEntityClass.Assassin) // TODO later dependent on assassin skill
        .Select(CharacterSimpleDto.FromCharacter)
      // others
    };

    await Clients.Caller.SendScoutQuadrantReport(report);

  }

  public async Task CharacterTeleportToNearest(CharacterCall characterCall) {
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    // TODO where to? position?hideout? quadrant? nearest town?
    character.XpLost = 0;
    character.UpdateStats((characterStats) => {
      characterStats.Hp.SetCurrentByPercent(50);
      characterStats.Mp.SetCurrentByPercent(25);
      return characterStats;
    });
    character.Status = CsEntityStatus.Awake;

    await _characterService.PersistAsync(character);

  }

  public async Task UseAction(CharacterUseActionCall characterUseActionCall) {
    if (!Enum.IsDefined(typeof(CharacterAction), characterUseActionCall.Action)) {
      return;
    }
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterUseActionCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    switch (characterUseActionCall.Action) {
      case CharacterAction.PickUp: {
        // NOT DONE
        break;
      }
      case CharacterAction.Attack: {
        if (character.Target is null) {
          break;
        }
        character.AttackTarget();
        break;
      }
      case CharacterAction.Sit: {
        if (character.Status != CsEntityStatus.Awake && character.Status != CsEntityStatus.Sitting) {
          break;
        }

        if (character.Status == CsEntityStatus.Awake) {
          character.Position.Stop();
          // TODO enhance regeneration
          character.Status = CsEntityStatus.Sitting;
        } else {
          character.Status = CsEntityStatus.Awake;
        }

        break;
      }
      case CharacterAction.Follow: {
        if (!character.CanApproachTarget()) {
          break;
        }

        character.FollowTarget();

        break;
      }
      default: return;
    }

  }

  public async Task TargetSelf(CharacterCall characterCall) {
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    character.TargetSelf();
  }

  public async Task TargetByInstanceId(CharacterTargetCall characterTargetCall) {
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterTargetCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    var targetCharacter = _characterService.FindCharacterByCsInstanceId(characterTargetCall.InstanceId);
    if (targetCharacter is null) {
      return;
    }

    // TODO search creature/actor instance

    character.Target = targetCharacter;
  }

  public async Task CancelTarget(CharacterCall characterCall) {
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    // TODO Check if this action is allowed
    character.CancelTarget();
  }

  public async Task AssignShortcut(CharacterAssignShortcutCall characterAssignShortcutCall) {
    if (!Enum.IsDefined(typeof(BarShortcutType), characterAssignShortcutCall.BarShortcutType) ||
      characterAssignShortcutCall.UsingId < 1 ||
      characterAssignShortcutCall.ShortcutIndex < 0 ||
      characterAssignShortcutCall.ShortcutIndex > 143) {
      return;
    }
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterAssignShortcutCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    await _characterService
      .AssignBarShortcutAsync(
        character,
        characterAssignShortcutCall.BarShortcutType,
        characterAssignShortcutCall.UsingId,
        characterAssignShortcutCall.ShortcutIndex);

    await Clients.Caller.UpdateBarShortcuts(character.BarShortcuts.Select(BarShortcutDto.FromBarShortcut));

  }

  public async Task ClearShortcut(CharacterClearShortcutCall characterClearShortcutCall) {
    if (characterClearShortcutCall.ShortcutIndex < 0 ||
      characterClearShortcutCall.ShortcutIndex > 143) {
      return;
    }
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterClearShortcutCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    await _characterService.RemoveBarShortcutAsync(character, characterClearShortcutCall.ShortcutIndex);

    await Clients.Caller.RemoveBarShortcut(characterClearShortcutCall.ShortcutIndex);

  }

  public async Task UseSkill(CharacterUseSkillCall characterUseSkillCall) {
    var (currentPlayer, character) = await GetPlayerAndCharacter(characterUseSkillCall);
    if (currentPlayer is null || character is null) {
      return;
    }

    var skillWrapper = character.SkillWrappers.SingleOrDefault(s => s.SkillKeyId == characterUseSkillCall.SkillKeyId);
    if (skillWrapper is null || skillWrapper.Skill is null || !character.CanUseSkill(skillWrapper)) {
      return;
    }

    // var isTargetInSkillRange = 


    // var affectedTargets = _characterService.GetSkillAffectedTargets(character, skillWrapper);

    character.UseSkill(skillWrapper, new List<ICsEntity>() { character.Target! });

  }

}

using System.Security.Claims;
using CS.Api.Communications;
using CS.Api.Communications.Models;
using CS.Api.Support.Attributes;
using CS.Api.Support.Models.Character;
using CS.Application.Options.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Support;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Controllers.Administration;
[ApiController]
[Route("[controller]")]
public class CharacterController : ControllerBase {

  private readonly IOptions<ApiBehaviorOptions> _apiBehaviorOptions;
  private readonly IPlayerService _playerService;
  private readonly ICharacterService _characterService;
  private readonly IHubContext<MainHub, ITypedHubClient> _mainHubContext;
  public CharacterController(
    IOptions<ApiBehaviorOptions> apiBehaviorOptions,
    IPlayerService playerService,
    ICharacterService characterService,
    IHubContext<MainHub, ITypedHubClient> mainHubContext) {
    _apiBehaviorOptions = Check.NotNull(apiBehaviorOptions, nameof(apiBehaviorOptions));
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
    _mainHubContext = Check.NotNull(mainHubContext, nameof(mainHubContext));
  }

  [HttpPost("create")]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 6, By = LimitRequestsType.NicknameCredentialAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Create(CharacterCreationDto characterCreationDto, CancellationToken cancellationToken) {

    var nickname = new Nickname((User.Identity as ClaimsIdentity)!.FindFirst("nickname")!.Value);
    var character = await _characterService
      .Create(
        nickname,
        new Nickname(characterCreationDto.Nickname),
        characterCreationDto.CharacterRace,
        characterCreationDto.CharacterClass,
        (byte)characterCreationDto.Gender,
        cancellationToken);

    if (character is null) {
      ModelState.AddModelError("Nickname", "Nickname already taken.");
      return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
    }

    var currentPlayer = await _playerService.GetPlayerAsync(nickname);
    await _mainHubContext.Clients.User(nickname.Value).SetCurrentPlayer(PlayerDto.FromPlayer(currentPlayer!));

    await _mainHubContext.Clients.User(nickname.Value).Reconnect();

    return NoContent();
  }

  [HttpPatch("Toggle")]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 18, By = LimitRequestsType.NicknameCredentialAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Toggle(CharacterToggleRequestDto characterToggleRequestDto, CancellationToken cancellationToken) {

    var playerNickname = new Nickname((User.Identity as ClaimsIdentity)!.FindFirst("nickname")!.Value);
    var characterNickname = new Nickname(characterToggleRequestDto.Nickname);

    var character = _characterService.Toggle(playerNickname, characterNickname, cancellationToken);
    if (character is not null) {
      await _mainHubContext.Clients.User(playerNickname.Value).UpdateCharacter(new { Id = character.Id, IsAwake = character.IsAwake });
    }

    return NoContent();

  }

}

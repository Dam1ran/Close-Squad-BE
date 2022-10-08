using System.Security.Claims;
using CS.Api.Communications;
using CS.Api.Support.Attributes;
using CS.Api.Support.Models.Character;
using CS.Application.Options.Abstractions;
using CS.Application.Persistence.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Services.Implementations;
using CS.Application.Support.Utils;
using CS.Core.Support;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class CharacterController : ControllerBase {

  private readonly IOptions<ApiBehaviorOptions> _apiBehaviorOptions;
  private readonly IPlayerService _playerService;
  private readonly ICharacterService _characterService;
  private readonly IHubContext<MainHub, ITypedHubClient> _mainHubContext;
  private readonly IServiceProvider _serviceProvider;

  public CharacterController(
    IOptions<ApiBehaviorOptions> apiBehaviorOptions,
    IPlayerService playerService,
    ICharacterService characterService,
    IHubContext<MainHub, ITypedHubClient> mainHubContext,
    IServiceProvider serviceProvider) {
    _apiBehaviorOptions = Check.NotNull(apiBehaviorOptions, nameof(apiBehaviorOptions));
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
    _mainHubContext = Check.NotNull(mainHubContext, nameof(mainHubContext));
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
  }

  [HttpPost("create")]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 6, By = LimitRequestsType.NicknameCredentialAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Create(CharacterCreationDto characterCreationDto, CancellationToken cancellationToken) {

    var nickname = new Nickname((User.Identity as ClaimsIdentity)!.FindFirst("nickname")!.Value);
    var characterNickname = new Nickname(characterCreationDto.Nickname);
    var player = await _playerService.GetOrCreatePlayerAsync(nickname, cancellationToken);

    using var scope = _serviceProvider.CreateScope();
    var _context = scope.ServiceProvider.GetRequiredService<IContext>();
    if (_context.Characters.Any(c => c.Nickname.ValueLowerCase == characterNickname.ValueLowerCase)) {
      ModelState.AddModelError("Nickname", "Nickname already taken.");
      return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
    }

    if (player.Characters.Count() >= CharacterService.MaxNumberOfCharacters) {
      ModelState.AddModelError("Creation", "Reached limit of characters.");
      return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
    }

    await _characterService
      .Create(
        player,
        characterNickname,
        characterCreationDto.CharacterClass,
        (byte)characterCreationDto.Gender,
        cancellationToken);

    await _mainHubContext.Clients.User(nickname.Value).Reconnect();

    return NoContent();
  }

}

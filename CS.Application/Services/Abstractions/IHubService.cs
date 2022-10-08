using CS.Core.Entities;

namespace CS.Application.Services.Abstractions;
public interface IHubService {

  public Task SendAllUpdateQuadrantPlayerList(Player player, bool toSelf = false);
  public Task SetCurrentPlayer(Player player);

}

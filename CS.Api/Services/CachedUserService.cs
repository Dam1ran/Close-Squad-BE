using CS.Api.Services.Abstractions;
using CS.Api.Support.Models;
using CS.Application.Models;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;

namespace CS.Api.Services;
public class CachedUserService : ICachedUserService {
  private readonly IServiceProvider _serviceProvider;
  private readonly ICacheService _cacheService;

  public CachedUserService(
    IServiceProvider serviceProvider,
    ICacheService cacheService)
  {
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  public async Task<CachedUser?> GetByNicknameAsync(Nickname nickname, CancellationToken cancellationToken) {
    var result = await _cacheService.GetAsync<CachedUser>(CacheGroupKeyConstants.User, nickname.Value, cancellationToken);
    if (result is not null) {
      return result;
    }

    CsUser? csUser;
    using (var scope = _serviceProvider.CreateScope()) {
      var _csUserRepo = scope.ServiceProvider.GetRequiredService<ICsUserRepository>();
      csUser = await _csUserRepo.FindByNicknameWithVerificationAsNoTrackingAsync(nickname, cancellationToken);
      if (csUser is null) {
        return null;
      }
    }

    var cachedUser = new CachedUser { CheckCaptcha = csUser.Verification.CheckCaptcha };

    await SetAsync(nickname.Value, cachedUser, cancellationToken);

    return cachedUser;
  }
  // to be set by admin action also when ready
  public async Task<UserManagerResponse> UpdateAsync(Nickname nickname, CachedUser cachedUser, CancellationToken cancellationToken) {
    Check.NotNull(cachedUser, nameof(cachedUser));

    using (var scope = _serviceProvider.CreateScope()) {
      var _csUserRepo = scope.ServiceProvider.GetRequiredService<ICsUserRepository>();
      var csUser = await _csUserRepo.FindByNicknameWithVerificationAsync(nickname, cancellationToken);
      if (csUser is null) {
        return UserManagerResponse.Failed("NonexistentUser", "No user with such nickname.");
      }

      if (cachedUser.CheckCaptcha)
      {
        csUser.Verification.EnableCheckCaptcha();
      }
      else
      {
        csUser.Verification.DisableCheckCaptcha();
      }

      await SetAsync(nickname.Value, cachedUser, cancellationToken);

      _csUserRepo.Update(csUser);
      await _csUserRepo.SaveChangesAsync(cancellationToken);

      return UserManagerResponse.Succeeded();
    }
  }

  private async Task SetAsync(string nickname, CachedUser cachedUser, CancellationToken cancellationToken) =>
    await _cacheService.SetAsync<CachedUser>(
      CacheGroupKeyConstants.User,
      nickname,
      cachedUser,
      absoluteExpirationRelativeToNow: TimeSpan.FromDays(1),
      slidingExpiration: TimeSpan.FromHours(4),
      cancellationToken: cancellationToken);

}

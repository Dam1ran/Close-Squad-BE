using CS.Api.Services.Abstractions;
using CS.Api.Support.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Utils;
using CS.Core.Entities.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace CS.Api.Services;
public class CachedUserService : ICachedUserService {
  private readonly IServiceProvider _serviceProvider;
  private readonly ICacheService _cacheService;

  public CachedUserService(
    IServiceProvider serviceProvider,
    ICacheService cacheService,
    IDataProtectionProvider dataProtectionProvider)
  {
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  public async Task<CachedUser?> GetByNicknameAsync(string nickname, CancellationToken cancellationToken) {
    var result = await _cacheService.GetAsync<CachedUser>(nickname, cancellationToken);
    if (result is not null) {
      return result;
    }

    User user;
    using (var scope = _serviceProvider.CreateScope()) {
      var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
      user = await _userManager.FindByNameAsync(nickname);
      if (user is null) {
        return null;
      }
    }

    var cachedUser = new CachedUser { CheckCaptcha = user.CheckCaptcha };

    await SetAsync(nickname, cachedUser, cancellationToken);

    return cachedUser;
  }
  // to be set by admin action also when ready
  public async Task<IdentityResult> UpdateAsync(string nickname, CachedUser cachedUser, CancellationToken cancellationToken) {
    Check.NotNullOrWhiteSpace(nickname, nameof(nickname));
    Check.NotNull(cachedUser, nameof(cachedUser));

    using (var scope = _serviceProvider.CreateScope()) {
      var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
      var user = await _userManager.FindByNameAsync(nickname);
      if (user is null) {
        return IdentityResult.Failed(new IdentityError { Code = "NonexistentUser", Description = "No user with such nickname" });
      }

      if (cachedUser.CheckCaptcha)
      {
        user.EnableCheckCaptcha();
      }
      else
      {
        user.DisableCheckCaptcha();
      }

      await SetAsync(nickname, cachedUser, cancellationToken);

      return await _userManager.UpdateAsync(user);
    }
  }

  private async Task SetAsync(string nickname, CachedUser cachedUser, CancellationToken cancellationToken) =>
    await _cacheService.SetAsync<CachedUser>(
      nickname,
      cachedUser,
      absoluteExpirationRelativeToNow: TimeSpan.FromDays(1),
      slidingExpiration: TimeSpan.FromHours(4),
      cancellationToken: cancellationToken);
}

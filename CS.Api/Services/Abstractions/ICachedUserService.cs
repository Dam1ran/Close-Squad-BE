using CS.Api.Support.Models;
using Microsoft.AspNetCore.Identity;

namespace CS.Api.Services.Abstractions;
public interface ICachedUserService {
  Task<CachedUser?> GetByNicknameAsync(string nickname, CancellationToken cancellationToken);
  Task<IdentityResult> UpdateAsync(string nickname, CachedUser cachedUser, CancellationToken cancellationToken);
}

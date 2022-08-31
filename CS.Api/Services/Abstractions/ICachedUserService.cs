using CS.Api.Support.Models;
using CS.Application.Models;
using CS.Core.ValueObjects;

namespace CS.Api.Services.Abstractions;
public interface ICachedUserService {
  Task<CachedUser?> GetByNicknameAsync(Nickname nickname, CancellationToken cancellationToken);
  Task<UserManagerResponse> UpdateAsync(Nickname nickname, CachedUser cachedUser, CancellationToken cancellationToken);
}

using CS.Application.Models;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IUserManager {

  Task<UserManagerResponse> CreateAsync(Nickname nickname, Email email, Password password, CancellationToken cancellationToken);
  Task<UserManagerResponse> SendConfirmationEmailAsync(Email email, CancellationToken cancellationToken);
  Task<UserManagerResponse> ConfirmEmailAsync(string guid, CancellationToken cancellationToken);
  Task<UserManagerResponse> LoginAsync(Email email, Password password, CancellationToken cancellationToken);
  Task<UserManagerResponse> SendChangePasswordEmailAsync(Email email, CancellationToken cancellationToken);
  Task<UserManagerResponse> ChangePasswordAsync(string guid, Password password, CancellationToken cancellationToken);
  Task<UserManagerResponse> RefreshTokenAsync(string sessionIdValue, string refreshTokenValue, CancellationToken cancellationToken);
  Task<UserManagerResponse> LogoutAsync(Nickname nickname, CancellationToken cancellationToken);

}

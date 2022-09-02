using CS.Application.DataTransferObjects;
using CS.Application.Models;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IUserManager {
  Task<UserManagerResponse> CreateAsync(Nickname nickname, Email email, Password password, CancellationToken cancellationToken);
  Task<UserManagerResponse> SendConfirmationEmailAsync(Email email, CancellationToken cancellationToken);
  Task<UserManagerResponse> ConfirmEmailAsync(string guid, CancellationToken cancellationToken);
  Task<UserManagerResponse> LoginAsync(Email email, Password password, CancellationToken cancellationToken);
  Task<UserManagerResponse> SendChangePasswordEmail(Email email, CancellationToken cancellationToken);
  // Task<string> CacheAndGetIdentificationToken(Nickname nickname, CancellationToken cancellationToken);

}

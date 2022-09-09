using System.Text.RegularExpressions;
using CS.Application.DataTransferObjects;
using CS.Application.Models;
using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.Entities.Auth;
using CS.Core.Extensions;
using CS.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CS.Application.Services.Implementations;
public class UserManager : IUserManager {
  public const int ProtectionTokenExpirationMinutes = 60;
  public const int ProtectionChangePasswordTokenExpirationMinutes = 10;

  private readonly ExternalInfoOptions _externalInfoOptions;
  private readonly ICsUserRepository _csUserRepo;
  private readonly ITemplatedEmailService _templatedEmailService;
  private readonly ICsTimeLimitedDataProtector _csTimeLimitedDataProtector;
  private readonly IUserTokenService _userTokenService;
  private readonly ICacheService _cacheService;
  private readonly ILogger<UserManager> _logger;

  public UserManager(
    IOptions<ExternalInfoOptions> externalInfoOptions,
    ICsUserRepository csUserRepo,
    ITemplatedEmailService templatedEmailService,
    ICsTimeLimitedDataProtector csTimeLimitedDataProtector,
    IUserTokenService userTokenService,
    ICacheService cacheService,
    ILogger<UserManager> logger)
  {
    _externalInfoOptions = Check.NotNull(externalInfoOptions?.Value, nameof(externalInfoOptions))!;
    _csUserRepo = Check.NotNull(csUserRepo, nameof(csUserRepo));
    _templatedEmailService = Check.NotNull(templatedEmailService, nameof(templatedEmailService));
    _csTimeLimitedDataProtector = Check.NotNull(csTimeLimitedDataProtector, nameof(csTimeLimitedDataProtector));
    _userTokenService = Check.NotNull(userTokenService, nameof(userTokenService));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task<UserManagerResponse> CreateAsync(Nickname nickname, Email email, Password password, CancellationToken cancellationToken) {

    var response = new UserManagerResponse();

    var isUsed = await IsUsed(nickname, email, response, cancellationToken);
    var isWeakPassword = IsWeakPassword(password.Value, response);
    if (isUsed || isWeakPassword) {
      return response;
    }

    await _csUserRepo.AddAsync(new CsUser(nickname, email, password), cancellationToken);
    await _csUserRepo.SaveChangesAsync(cancellationToken);

    return response;

  }

  private async Task<bool> IsNicknameUsed(Nickname nickname, CancellationToken cancellationToken) =>
    await _csUserRepo.AnyByNicknameAsNoTrackingAsync(nickname, cancellationToken);

  private async Task<bool> IsEmailUsed(Email email, CancellationToken cancellationToken) =>
    await _csUserRepo.AnyByEmailAsNoTrackingAsync(email, cancellationToken);

  private async Task<bool> IsUsed(Nickname nickname, Email email, UserManagerResponse userManagerResponse, CancellationToken cancellationToken) {

    var isNicknameUsed = await IsNicknameUsed(nickname, cancellationToken);
    var isEmailUsed = await IsEmailUsed(email, cancellationToken);

    if (isNicknameUsed) {
      userManagerResponse.AddErrorDetails("Nickname", "Nickname already taken.");
    }

    if (isEmailUsed) {
      userManagerResponse.AddErrorDetails("Email", "Email already in use.");
    }

    return isNicknameUsed || isEmailUsed;

  }
  private bool IsWeakPassword(string password, UserManagerResponse userManagerResponse) {

    Match match = (new Regex(Password.RegexPattern)).Match(password);

    if (
      !match.Success ||
      match.Index != 0 ||
      match.Length != password.Length ||
      password.Length < Password.MinLength ||
      password.Length > Password.MaxLength)
    {
      userManagerResponse.AddErrorDetails("Password", "Password does not met complexity requirements.");
      return true;
    }

    return false;

  }

  public async Task<UserManagerResponse> SendConfirmationEmailAsync(Email email, CancellationToken cancellationToken) {
    var csUser = await _csUserRepo.FindByEmailAsNoTrackingAsync(email, cancellationToken);
    if (csUser is null || csUser.Verification.Banned) {
      return UserManagerResponse.Failed("NonexistentUser", "User with specified email does not exists.");
    }

    if (csUser.Verification.EmailConfirmed) {
      return UserManagerResponse.Failed("EmailAlreadyConfirmed", "Email already confirmed.");
    }

    var sendEmailResponse = await _templatedEmailService
      .SendConfirmationAsync(
        csUser.Verification.Email.Value,
        csUser.Nickname.Value,
        CreateConfirmationLink(csUser.Nickname));

    if (!sendEmailResponse.Successful) {
      foreach(var err in sendEmailResponse.Errors) {
        _logger.LogWarning($"Email confirmation error: {err}");
      }
      return UserManagerResponse.Failed("SendEmailFailed", "Failed to send email.");
    }

    return UserManagerResponse.Succeeded();
  }

  private string CreateConfirmationLink(Nickname nickname) {
    var protectedUserNickname =
      _csTimeLimitedDataProtector
        .ProtectNickname(
          nickname,
          TimeSpan.FromMinutes(ProtectionTokenExpirationMinutes));

    return $"{_externalInfoOptions.ConfirmAddressLink}?guid={protectedUserNickname}";
  }

  public async Task<UserManagerResponse> ConfirmEmailAsync(string guid, CancellationToken cancellationToken) {

    var failedResponse = UserManagerResponse.Failed("WrongData", "Provided data did not yeld any result.");

    var nickname = _csTimeLimitedDataProtector.UnprotectNickname(guid, out bool expired);
    if (expired) {
      return UserManagerResponse.Failed("TokenExpired", "Confirmation link expired.");
    }

    if (nickname is null) {
      return failedResponse;
    }

    var csUser = await _csUserRepo.FindByNicknameWithVerificationAsync(nickname, cancellationToken);
    if (csUser is null || csUser.Verification.Banned) {
      return failedResponse;
    }

    if (csUser.Verification.EmailConfirmed) {
      return UserManagerResponse.Failed("AlreadyConfirmed", "Email already confirmed.");
    }

    csUser.Verification.SetEmailConfirmed();
    _csUserRepo.Update(csUser);
    await _csUserRepo.SaveChangesAsync(cancellationToken);

    return UserManagerResponse.Succeeded();

  }

  public async Task<UserManagerResponse> LoginAsync(Email email, Password password, CancellationToken cancellationToken) {

    var response = UserManagerResponse.Succeeded();

    var csUser = await _csUserRepo.FindByEmailWithAuthAsync(email, cancellationToken);
    if (csUser is null) {
      response.AddErrorDetails("WrongCredentials", "Wrong credentials provided."); // wrong email
      return response;
    }

    var passwordObj = csUser.Identification.IdentificationPassword;

    if (passwordObj.IsLockedOut()) {

      if (!csUser.Verification.LockoutEmailSent) {
        _logger.LogWarning($"The email of user {csUser.Nickname} has been locked out.");
        await _templatedEmailService.SendAccountLockedOutAsync(email.Value, csUser.Nickname.Value);
        csUser.Verification.LockoutEmailSent = true;
        await _csUserRepo.SaveChangesAsync(cancellationToken);
      }

      var lockoutMinutes = (int)Math.Ceiling((passwordObj.LockoutEndsAt - DateTimeOffset.UtcNow).TotalMinutes);
      response.AddErrorDetails(
        "LockedOut",
        $"Too many attempts to login. Locked out for {lockoutMinutes} minute{(lockoutMinutes == 1 ? "." : "s.")}");
      response.IntegerData = lockoutMinutes;

      return response;
    }

    if (!passwordObj.CheckPasswordAndRecordAttempt(password)) {
      response.AddErrorDetails("WrongCredentials", "Wrong credentials provided."); // wrong password
      await _csUserRepo.SaveChangesAsync(cancellationToken);
      return response;
    }

    if (csUser.Verification.Banned) {
      response.AddErrorDetails("UserBanned", $"User banned for: {csUser.Verification.BanReason}");
      return response;
    }

    if (!csUser.Verification.EmailConfirmed) {
      response.AddErrorDetails("EmailNotConfirmed", "Email not confirmed. Please check your email inbox and spam folder.");
      return response;
    }

    passwordObj.ResetAttempts();
    csUser.Verification.LockoutEmailSent = false;
    response.IntegerData = (int)Math.Ceiling(csUser.Verification.SetLoginDateTimeAndGetInterval().TotalSeconds);

    var irt = await _userTokenService.CreateAndCacheIrtAsync(csUser, cancellationToken);
    csUser.Identification.SetRefreshToken(irt);
    response.RefreshToken = IdentificationRefreshTokenDto.FromIrt(irt);

    response.Token = await _userTokenService.CreateAndCacheItAsync(csUser.Nickname, csUser.Identification.Role, cancellationToken);

    await _csUserRepo.SaveChangesAsync(cancellationToken);

    return response;

  }

  public async Task<UserManagerResponse> SendChangePasswordEmailAsync(Email email, CancellationToken cancellationToken) {
    var csUser = await _csUserRepo.FindByEmailAsNoTrackingAsync(email, cancellationToken);
    if (csUser is null || csUser.Verification.Banned) {
      return UserManagerResponse.Failed("WrongCredentials", "Wrong credentials provided.");
    }

    var sendEmailResponse = await _templatedEmailService
      .SendResetPasswordAsync(
        csUser.Verification.Email.Value,
        csUser.Nickname.Value,
        CreateChangePasswordLink(csUser.Nickname));

    if (!sendEmailResponse.Successful) {
      foreach(var err in sendEmailResponse.Errors) {
        _logger.LogWarning($"Email confirmation error: {err}");
      }

      return UserManagerResponse.Failed("SendEmailFailed", "Failed to send email.");

    }

    return UserManagerResponse.Succeeded();

  }

  private string CreateChangePasswordLink(Nickname nickname) {
    var protectedUserNickname =
      _csTimeLimitedDataProtector
        .ProtectNickname(
          nickname,
          TimeSpan.FromMinutes(ProtectionChangePasswordTokenExpirationMinutes));

    return $"{_externalInfoOptions.ChangePasswordLink}?guid={protectedUserNickname}";
  }

  public async Task<UserManagerResponse> ChangePasswordAsync(string guid, Password password, CancellationToken cancellationToken) {

    var nickname = _csTimeLimitedDataProtector.UnprotectNickname(guid, out bool expired);
    if (expired) {
      return UserManagerResponse.Failed("TokenExpired", "Change password link expired.");
    }

    var wrongDataResponse = UserManagerResponse.Failed("WrongData", "Provided data did not yeld any result.");
    if (nickname is null) {
      return wrongDataResponse;
    }

    var csUser = await _csUserRepo.FindByNicknameWithVerificationAndIdentificationPasswordAsync(nickname, cancellationToken);
    if (csUser is null || csUser.Verification.Banned) {
      return wrongDataResponse;
    }

    if (csUser.Identification.IdentificationPassword.CheckPassword(password)) {
      return UserManagerResponse.Failed("SamePassword", "Password must be different than previous one.");
    }

    csUser.Identification.SetIdentificationPassword(password);

    await _csUserRepo.SaveChangesAsync(cancellationToken);

    return UserManagerResponse.Succeeded();

  }

  public async Task<UserManagerResponse> RefreshTokenAsync(string refreshTokenValue, CancellationToken cancellationToken) {

    var failedResult = UserManagerResponse.Failed("WrongRefreshToken", "Refresh token is expired or wrong.");
    var (nickname, role) = _csTimeLimitedDataProtector.UnprotectNicknameAndRole(refreshTokenValue, out bool expired);
    if (expired || nickname is null) {
      return failedResult;
    }

    var cachedRefreshToken = await _cacheService.GetStringAsync(CacheGroupKeyConstants.UserRefreshToken, nickname.Value, cancellationToken);

    if (string.IsNullOrWhiteSpace(cachedRefreshToken)) {
      CsUser? csUser = await _csUserRepo.FindByNicknameWithVerificationAndIdentificationRefreshTokenAsNoTrackingAsync(nickname, cancellationToken);
      if (csUser is null || csUser.Verification.Banned) {
        _logger.LogWarning($"Attempt to find a banned or nonexistent user after decrypted token!");
        return failedResult;
      }

      if (!csUser.Identification.IdentificationRefreshToken.ExpiresAt.IsInFuture()) {
        return failedResult;
      }

      cachedRefreshToken = csUser.Identification.IdentificationRefreshToken.RefreshToken;
      role = csUser.Identification.Role;

      await _cacheService.SetStringAsync(
        CacheGroupKeyConstants.UserRefreshToken,
        csUser.Nickname.Value,
        cachedRefreshToken,
        absoluteExpiration: csUser.Identification.IdentificationRefreshToken.ExpiresAt,
        cancellationToken: cancellationToken);

    }


    if (!cachedRefreshToken.SequenceEqual(refreshTokenValue)) {
      return failedResult;
    }

    var succeededResponse = UserManagerResponse.Succeeded();
    succeededResponse.Token = await _userTokenService.CreateAndCacheItAsync(nickname, role, cancellationToken);

    return succeededResponse;
  }

  public async Task<UserManagerResponse> LogoutAsync(Nickname nickname, CancellationToken cancellationToken) {

    var failedResult = UserManagerResponse.Failed("CannotLogout", "Error has occurred while trying to log out.");
    var csUser = await _csUserRepo.FindByNicknameWithVerificationAndIdentificationRefreshTokenAsync(nickname, cancellationToken);
    if (csUser is null) {
      _logger.LogWarning($"Attempt to find a nonexistent user after decrypted token!");
      return failedResult;
    }

    await _cacheService.RemoveAsync(CacheGroupKeyConstants.UserRefreshToken, nickname.Value, cancellationToken);
    await _cacheService.RemoveAsync(CacheGroupKeyConstants.UserJwt, nickname.Value, cancellationToken);

    csUser.Identification.IdentificationRefreshToken.Clear();
    await _csUserRepo.SaveChangesAsync(cancellationToken);

    return UserManagerResponse.Succeeded();

  }

}

using System.Security.Cryptography;
using System.Web;
using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Utils;
using CS.Core.Entities.Auth;
using CS.Infrastructure.Models;
using CS.Infrastructure.Services.Abstractions;
using CS.Infrastructure.Support.Constants;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CS.Infrastructure.Services;
public class UserService : IUserService {
  private readonly ExternalInfoOptions _externalInfoOptions;
  private readonly UserManager<User> _userManager;
  // private readonly SignInManager<User> _signInManager;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ITemplatedEmailService _templatedEmailService;
  private readonly IDataProtector _dataProtector;
  private readonly ILogger<UserService> _logger;

  public UserService(
    IOptions<ExternalInfoOptions> externalInfoOptions,
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    ITemplatedEmailService templatedEmailService,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<UserService> logger) {
    _externalInfoOptions = Check.NotNull(externalInfoOptions?.Value, nameof(externalInfoOptions))!;
    _userManager = Check.NotNull(userManager, nameof(userManager));
    _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
    _templatedEmailService = Check.NotNull(templatedEmailService, nameof(templatedEmailService));
    _dataProtector = dataProtectionProvider.CreateProtector(Infrastructure_DataProtectionPurposeStringsConstants.UserKeyEncryption);
    _logger = Check.NotNull(logger, nameof(logger));
  }
  public async Task<IdentityResult> CreateUser(string nickname, string email, string password) {
    return await _userManager.CreateAsync(new User(nickname, email), password);
  }

  public async Task<SendEmailResponse> SendConfirmationEmail(string email) {

    var user = await _userManager.FindByEmailAsync(email);
    if (user is null) {
      return SendEmailResponse.Failed("User with specified email does not exists.");
    }

    if (user.EmailConfirmed) {
      return SendEmailResponse.Failed("Email already confirmed.");
    }

    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

    return await _templatedEmailService
      .SendConfirmationAsync(user.Email, user.UserName, CreateConfirmationLink(user.UserName, confirmationToken));
  }

  private string CreateConfirmationLink(string userKey, string confirmationToken) {
    var protectedUserNickname = HttpUtility.UrlEncode(_dataProtector.Protect(userKey));
    var token = HttpUtility.UrlEncode(_dataProtector.Protect(confirmationToken));

    return $"{_externalInfoOptions.ConfirmAddressLink}?guid={protectedUserNickname}&token={token}";
  }

  public async Task<IdentityResult> ConfirmEmail(string guid, string token) {
    try {
      guid = _dataProtector.Unprotect(HttpUtility.UrlDecode(guid));
      token = _dataProtector.Unprotect(HttpUtility.UrlDecode(token));
    } catch (CryptographicException ex) {
      _logger.LogWarning(ex.Message);
      return IdentityResult.Failed(new IdentityError { Code = "WrongData", Description = "Provided data did not yeld any result." });
    }

    var user = await _userManager.FindByNameAsync(guid);
    if (user is null) {
      return IdentityResult.Failed(new IdentityError { Code = "WrongData", Description = "Provided data did not yeld any result." });
    }

    if (user.EmailConfirmed) {
      return IdentityResult.Failed(new IdentityError { Code = "AlreadyConfirmed", Description = "Email already confirmed." });
    }

    return await _userManager.ConfirmEmailAsync(user, token);
  }

}

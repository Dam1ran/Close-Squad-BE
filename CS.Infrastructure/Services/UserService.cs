using System.Web;
using CS.Application.Exceptions;
using CS.Application.Utils;
using CS.Core.Entities.Auth;
using CS.Infrastructure.Exceptions;
using CS.Infrastructure.Models;
using CS.Infrastructure.Services.Abstractions;
using CS.Infrastructure.Support.Constants;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace CS.Infrastructure.Services;
public class UserService : IUserService {
  private readonly UserManager<User> _userManager;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ITemplatedEmailService _templatedEmailService;
  private readonly IConfiguration _configuration;
  private readonly IDataProtector _dataProtector;
  public UserService(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    ITemplatedEmailService templatedEmailService,
    IConfiguration configuration,
    IDataProtectionProvider dataProtectionProvider) {
    _userManager = Check.NotNull(userManager, nameof(userManager));
    _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
    _templatedEmailService = Check.NotNull(templatedEmailService, nameof(templatedEmailService));
    _configuration = Check.NotNull(configuration, nameof(configuration));
    _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposeStringsConstants.UserIdRouteValue);
  }
  public async Task<IdentityResult> CreateUser(string nickname, string email, string password) {
    var user = new User {
      UserName = nickname,
      Email = email,
      EmailConfirmed = false
    };
    return await _userManager.CreateAsync(user, password);
  }

  public async Task<SendEmailResponse> SendConfirmationEmail(string email) {
    var user = await _userManager.FindByEmailAsync(email);
    if (user is null) {
      return SendEmailResponse.Failed("User with specified email does not exists.");
    }

    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

    return await _templatedEmailService
      .SendConfirmationAsync(user.Email, user.UserName, CreateConfirmationLink(user.Id, confirmationToken));
  }

  private string CreateConfirmationLink(long userId, string confirmationToken) {
    var confirmAddress = _configuration["ConfirmAddressLink"];
    var protectedUserId = HttpUtility.UrlEncode(_dataProtector.Protect(userId.ToString()));
    var token = HttpUtility.UrlEncode(_dataProtector.Protect(confirmationToken));

    return $"{confirmAddress}?guid={protectedUserId}&token={token}";
  }
}
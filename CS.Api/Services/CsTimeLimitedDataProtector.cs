using System.Security.Cryptography;
using System.Web;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.DataProtection;

namespace CS.Api.Services;
public class CsTimeLimitedDataProtector : ICsTimeLimitedDataProtector {

  private readonly IDataProtectionProvider _dataProtectionProvider;
  private readonly ILogger<CsTimeLimitedDataProtector> _logger;

  public CsTimeLimitedDataProtector(IDataProtectionProvider dataProtectionProvider, ILogger<CsTimeLimitedDataProtector> logger) {
    _dataProtectionProvider = Check.NotNull(dataProtectionProvider, nameof(dataProtectionProvider));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public string ProtectNickname(Nickname nickname, TimeSpan forRelativeToNow) {

    var paddedNickname = nickname.Value.PadLeft(20, '*');
    var protectedNickname = GetNicknameProtector().Protect(paddedNickname, DateTimeOffset.UtcNow.Add(forRelativeToNow));
    var encodedNickname = HttpUtility.UrlEncode(protectedNickname);

    return encodedNickname;

  }

  public Nickname? UnprotectNickname(string value, out bool expired) {
    var expiresAt = DateTimeOffset.MinValue;
    Nickname? nickname = null;

    try {

      var unprotectedNickname = GetNicknameProtector().Unprotect(HttpUtility.UrlDecode(value), out expiresAt);
      var trimmedNickname = unprotectedNickname.TrimStart('*');

      nickname = new Nickname(trimmedNickname);

      expired = false;

      return nickname;

    } catch (CryptographicException ex) {
      _logger.LogWarning(ex.Message);
      expired = ex.InnerException == null;
    }

    return nickname;

  }

  public string ProtectNicknameAndRole(Nickname nickname, string role, string sessionIdValue, TimeSpan forRelativeToNow) {

    var protectedValue = GetNicknameProtector().Protect($"{role}*{nickname.Value}*{sessionIdValue}", DateTimeOffset.UtcNow.Add(forRelativeToNow));
    var encodedValue = HttpUtility.UrlEncode(protectedValue);

    return encodedValue;
  }

  public (Nickname?, string, string) UnprotectNicknameAndRole(string value, out bool expired) {
    var expiresAt = DateTimeOffset.MinValue;
    Nickname? nickname = null;
    var role = string.Empty;
    var sessionIdValue = string.Empty;

    try {

      var unprotectedValue = GetNicknameProtector().Unprotect(HttpUtility.UrlDecode(value), out expiresAt);
      var parts = unprotectedValue.Split('*');

      expired = false;

      if (parts.Length == 3) {
        role = parts[0];
        nickname = new Nickname(parts[1]);
        sessionIdValue = parts[2];


        return (nickname, role, sessionIdValue);
      }


    } catch (CryptographicException ex) {
      _logger.LogWarning(ex.Message);
      expired = ex.InnerException == null;
    }

    return (nickname, role, sessionIdValue);
  }


  private ITimeLimitedDataProtector GetNicknameProtector() {
    var protector =
      _dataProtectionProvider
      .CreateProtector(Application_DataProtectionPurposeStringsConstants.UserKeyEncryption);

    return protector.ToTimeLimitedDataProtector();

  }

}

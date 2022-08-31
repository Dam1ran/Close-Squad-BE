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
  public string ProtectNickname(Nickname nickname, TimeSpan forRelativeToNow) =>
    HttpUtility.UrlEncode(GetNicknameProtector().Protect(nickname.Value, DateTimeOffset.UtcNow.Add(forRelativeToNow)));

  public Nickname? UnprotectNickname(string value, out bool expired) {
    var expiresAt = DateTimeOffset.MinValue;
    Nickname? nickname = null;

    try {
      nickname = new Nickname(
        GetNicknameProtector()
        .Unprotect(HttpUtility.UrlDecode(value), out expiresAt));

      expired = false;

      return nickname;

    } catch (CryptographicException ex) {
      _logger.LogWarning(ex.Message);
      expired = ex.InnerException == null;
    }

    return nickname;

  }

  private ITimeLimitedDataProtector GetNicknameProtector() {
    var protector =
      _dataProtectionProvider
      .CreateProtector(Application_DataProtectionPurposeStringsConstants.UserKeyEncryption);

    return protector.ToTimeLimitedDataProtector();

  }

}

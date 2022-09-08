using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;
using Microsoft.IdentityModel.Tokens;

namespace CS.Infrastructure.Services;
public class UserTokenService : IUserTokenService {
  private readonly RefreshTokenOptions _refreshTokenOptions;
  private readonly JwtOptions _jwtOptions;
  private readonly ICacheService _cacheService;
  private readonly ICsTimeLimitedDataProtector _csTimeLimitedDataProtector;

  public UserTokenService(
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    IOptions<JwtOptions> jwtOptions,
    ICacheService cacheService,
    ICsTimeLimitedDataProtector csTimeLimitedDataProtector)
  {
    _refreshTokenOptions = Check.NotNull(refreshTokenOptions?.Value, nameof(refreshTokenOptions))!;
    _jwtOptions = Check.NotNull(jwtOptions?.Value, nameof(jwtOptions))!;
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _csTimeLimitedDataProtector = Check.NotNull(csTimeLimitedDataProtector, nameof(csTimeLimitedDataProtector));
  }

  public async Task<IdentificationRefreshToken> CreateAndCacheIrtAsync(CsUser csUser, CancellationToken cancellationToken) {

    var rtExpiration = TimeSpan.FromMinutes(_refreshTokenOptions.ExpiresInMinutes);

    var protectedValue =
      _csTimeLimitedDataProtector
        .ProtectNicknameAndRole(
          csUser.Nickname,
          csUser.Identification.Role,
          rtExpiration);

    var tokenExpiration = DateTimeOffset.UtcNow.AddMinutes(_refreshTokenOptions.ExpiresInMinutes);

    var irt = new IdentificationRefreshToken(protectedValue, tokenExpiration);

    await _cacheService.SetStringAsync(
      CacheGroupKeyConstants.UserRefreshToken,
      csUser.Nickname.Value,
      irt.RefreshToken,
      absoluteExpirationRelativeToNow: rtExpiration,
      cancellationToken: cancellationToken);

    return irt;

  }

  public async Task<string> CreateAndCacheItAsync(Nickname nickname, string role, CancellationToken cancellationToken) {

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

    var guid = Guid.NewGuid().ToString();
    List<Claim> claims = new List<Claim> {
      new Claim(JwtRegisteredClaimNames.Jti, guid),
      new Claim("nickname", nickname.Value),
      new Claim("role", role)
    };

    var token = new JwtSecurityToken(
      issuer: _jwtOptions.ValidIssuer,
      audience: _jwtOptions.ValidAudience,
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes),
      signingCredentials: credentials);

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    await _cacheService.SetStringAsync(
      CacheGroupKeyConstants.UserJwt,
      nickname.Value,
      guid,
      absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(_jwtOptions.ExpiresInMinutes),
      cancellationToken: cancellationToken);

    return jwt;

  }

}

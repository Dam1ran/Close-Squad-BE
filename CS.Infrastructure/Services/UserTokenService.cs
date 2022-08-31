using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.Entities.Auth;
using Microsoft.IdentityModel.Tokens;

namespace CS.Infrastructure.Services;
public class UserTokenService : IUserTokenService {
  private readonly RefreshTokenOptions _refreshTokenOptions;
  private readonly JwtOptions _jwtOptions;
  private readonly ICacheService _cacheService;
  public UserTokenService(
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    IOptions<JwtOptions> jwtOptions,
    ICacheService cacheService)
  {
    _refreshTokenOptions = Check.NotNull(refreshTokenOptions?.Value, nameof(refreshTokenOptions))!;
    _jwtOptions = Check.NotNull(jwtOptions?.Value, nameof(jwtOptions))!;
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  public async Task<IdentificationRefreshToken> CreateAndCacheIrt(CsUser csUser, CancellationToken cancellationToken) {
    var irt = new IdentificationRefreshToken(
      Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
      DateTimeOffset.UtcNow.AddMinutes(_refreshTokenOptions.ExpiresInMinutes));

    await _cacheService.SetStringAsync(
      CacheGroupKeyConstants.UserRefreshToken,
      csUser.Nickname.Value,
      irt.RefreshToken,
      absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(_refreshTokenOptions.ExpiresInMinutes),
      cancellationToken: cancellationToken);

    return irt;
  }

  public async Task<string> CacheAndGetIdentificationToken(CsUser csUser, CancellationToken cancellationToken) {

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

    var guid = Guid.NewGuid().ToString();
    List<Claim> claims = new List<Claim> {
      new Claim(JwtRegisteredClaimNames.Jti, guid),
      new Claim("nickname", csUser.Nickname.Value),
      new Claim("role", csUser.Identification.Role)
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
      csUser.Nickname.Value,
      guid,
      absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(_jwtOptions.ExpiresInMinutes),
      cancellationToken: cancellationToken);

    return jwt;

  }

}

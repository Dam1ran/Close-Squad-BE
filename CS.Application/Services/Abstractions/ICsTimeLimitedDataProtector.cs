using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface ICsTimeLimitedDataProtector {

  string ProtectNickname(Nickname nickname, TimeSpan forRelativeToNow);
  Nickname? UnprotectNickname(string value, out bool expired);
  string ProtectNicknameAndRole(Nickname nickname, string role, string sessionIdValue, TimeSpan forRelativeToNow);
  (Nickname?, string, string) UnprotectNicknameAndRole(string value, out bool expired);

}

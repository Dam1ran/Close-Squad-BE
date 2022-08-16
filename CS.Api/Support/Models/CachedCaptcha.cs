namespace CS.Api.Support.Models;
public class CachedCaptcha {
  public string Keycode { get; set; } = string.Empty;
  public int Attempts { get; set; } = 0;
}

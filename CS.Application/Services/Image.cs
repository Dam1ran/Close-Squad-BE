namespace CS.Application.Services;
public class Image : CsFile {
  public static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase) {
    { ".jpeg", "image/jpeg" },
    { ".jpg", "image/jpeg" },
    { ".png", "image/png" }
  };

  public Image(string name, MemoryStream stream) : base(name, ContentTypes[Path.GetExtension(name)], stream) {}
}

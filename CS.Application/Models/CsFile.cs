using CS.Application.Models.Abstractions;
using CS.Application.Support.Utils;

namespace CS.Application.Models;
public class CsFile : ICsFile {
  public string Name { get; protected set; }
  public string ContentType { get; protected set; }
  public long ContentLength { get; protected set; }
  public double Size => ContentLength / (1024 * 1024);
  public MemoryStream Stream { get; protected set; }

  public CsFile(string name, string contentType, MemoryStream stream) {
    Name = Check.NotNull(name, nameof(name));
    ContentType = Check.NotNull(contentType, nameof(contentType));
    Stream = Check.NotNull(stream, nameof(stream));

    Stream.Position = 0;
  }

  public CsFile(string name, string contentType, long contentLength, MemoryStream stream)
    : this(name, contentType, stream)
  {
    ContentLength = contentLength;
  }

}

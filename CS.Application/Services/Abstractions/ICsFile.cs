namespace CS.Application.Services.Abstractions;
public interface ICsFile {
  string Name { get; }
  string ContentType { get; }
  long ContentLength { get; }
  MemoryStream Stream { get; }
}

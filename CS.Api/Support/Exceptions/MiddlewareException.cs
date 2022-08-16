namespace CS.Api.Support.Exceptions;
[Serializable]
public class MiddlewareException : Exception {
  public MiddlewareException(string middlewareName, string message): base($"[{middlewareName}] {message}") {}
}

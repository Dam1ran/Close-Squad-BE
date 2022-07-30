namespace CS.Core.Exceptions;
[Serializable]
public class AlreadyExistsException : Exception {
  public AlreadyExistsException(string message): base(message) {}
}

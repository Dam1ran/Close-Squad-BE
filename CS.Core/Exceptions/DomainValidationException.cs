namespace CS.Core.Exceptions;
[Serializable]
public class DomainValidationException : Exception {
  public DomainValidationException(string message): base(message) {}
}

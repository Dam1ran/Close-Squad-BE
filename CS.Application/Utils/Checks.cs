namespace CS.Application.Utils;
public static class Check {
  public static T NotNull<T>(T value, string parameterName) {
    if (value == null) {
      NotNullOrWhiteSpace(parameterName, nameof(parameterName));

      throw new ArgumentNullException(parameterName);
    }

    return value;
  }

  public static string NotNullOrWhiteSpace(string value, string parameterName) {
    if (string.IsNullOrWhiteSpace(value)) {
      NotNullOrWhiteSpace(parameterName, nameof(parameterName));

      throw new ArgumentNullException(parameterName);
    }

    return value;
  }
}

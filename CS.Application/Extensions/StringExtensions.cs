namespace CS.Application.Extensions;
public static class StringExtensions {
  public static bool EqualsExceptNCharacters(this string comparer, string compareTo, int numberOfCharacters) {
    if (comparer.Length != compareTo.Length) {
      return false;
    }

    comparer = comparer.ToUpper();
    compareTo = compareTo.ToUpper();

    var count = 0;
    for (int i = 0; i < comparer.Length; i++) {
      if (comparer[i] != compareTo[i]) {
        count++;
      }
    }

    return numberOfCharacters >= count;
  }
}

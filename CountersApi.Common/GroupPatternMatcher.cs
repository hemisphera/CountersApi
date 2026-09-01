using System.Text.RegularExpressions;

namespace CountersApi.Common;

public static class GroupPatternMatcher
{
  /// <summary>
  ///   Returns <c>true</c> when <paramref name="group" /> matches the supplied
  ///   regex <paramref name="pattern" />. A null or empty pattern matches all
  ///   groups (equivalent to <c>.*</c>).
  /// </summary>
  public static bool Matches(string? pattern, string group)
  {
    if (string.IsNullOrEmpty(pattern)) return true;
    return Regex.IsMatch(group, pattern);
  }
}
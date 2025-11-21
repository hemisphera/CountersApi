namespace CounterAPI.Common;

public static class StringExtensions
{
  extension(string str)
  {
    public string Sanitize()
    {
      char[] invalidChars = ['<', '>', ':', '"', '/', '\\', '|', '?', '*', ' ', '\t', '\n', '\r'];
      var sanitized = str.ToLowerInvariant();
      return invalidChars.Aggregate(sanitized, (current, c) => current.Replace(c.ToString(), string.Empty));
    }

    public string? NullIfWhitespace()
    {
      return string.IsNullOrWhiteSpace(str) ? null : str;
    }
  }
}
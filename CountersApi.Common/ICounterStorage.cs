namespace CountersApi.Common;

public interface ICounterStorage
{
  Task<CounterValue?> Get(string group, string name);
  Task Set(string group, string name, CounterValue value);
  Task<IEnumerable<string>> List(string group);

  /// <summary>
  ///   Validates the optional API key for a group. When the API key store is
  ///   not configured, all access is allowed. When configured, a valid
  ///   <paramref name="apiKey" /> is required whose group-pattern matches
  ///   <paramref name="group" />.
  /// </summary>
  Task<bool> IsAuthorized(string group, string? apiKey);
}
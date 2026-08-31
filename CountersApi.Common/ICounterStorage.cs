namespace CountersApi.Common;

public interface ICounterStorage
{
  Task<CounterValue?> Get(string group, string name);
  Task Set(string group, string name, CounterValue value);
  Task<IEnumerable<string>> List(string group);
}
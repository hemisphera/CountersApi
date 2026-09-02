using CountersApi.Common;

namespace CountersApi.Models;

public class CounterStateDto
{
  public string Group { get; }
  public string Name { get; }
  public CounterValue Value { get; }
  public bool? WasModified { get; }


  public CounterStateDto(string group, string name, CounterValue value, bool? wasModified = null)
  {
    Group = group;
    Name = name;
    Value = value;
    WasModified = wasModified;
  }
}
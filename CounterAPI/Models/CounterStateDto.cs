using System.Text.Json.Serialization;

namespace CounterAPI.Models;

[JsonSerializable(typeof(CounterStateDto))]
public class CounterStateDto
{
  public string Group { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Signature { get; set; } = string.Empty;
  public int Value { get; set; }
}
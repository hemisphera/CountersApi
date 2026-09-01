namespace CountersApi.Models;

public class CounterRequest
{
  public long? Value { get; set; }
  public string? Signature { get; set; }
  public long? Seed { get; set; }
  public int Increment { get; set; } = 1;
}
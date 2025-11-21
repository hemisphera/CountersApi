namespace CounterAPI.Common;

public readonly struct CounterValue
{
  public long Value { get; }
  public string? Signature { get; }

  public CounterValue(long value, string? signature = null)
  {
    Value = value;
    Signature = signature;
  }
}
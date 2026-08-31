using System;

namespace CountersApi.Deploy;

public class Globals
{
  public static string EnvironmentName { get; set; } = "dev";
  public static readonly Guid InternalId = new("b7130262-f230-47ac-8b56-581ceea258c3");
  public static readonly string Name = "counter-api";
}
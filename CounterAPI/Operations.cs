using CounterAPI.Common;
using CounterAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CounterAPI;

public static class Operations
{
  public static async Task<IResult> HandleGet([FromRoute] string group, [FromRoute] string name, ICounterStorage storage)
  {
    var current = await storage.Get(group, name);
    if (current == null) return Results.NotFound();
    return Results.Ok(new
    {
      current.Value.Value,
      current.Value.Signature
    });
  }

  public static async Task<IResult> HandleList([FromRoute] string group, ICounterStorage storage)
  {
    var items = await storage.List(group);
    return Results.Ok(items);
  }

  public static async Task<IResult> HandleSet([FromRoute] string group, [FromRoute] string name, [FromBody] CounterRequest request, ICounterStorage storage)
  {
    if (request.Value != null)
    {
      var result = request.Value.Value;
      await storage.Set(group, name, new CounterValue(request.Value.Value, request.Signature));
      return Results.Ok(result);
    }

    var existing = await storage.Get(group, name);
    var currValue = existing?.Value ?? request.Seed ?? 0;

    if (SignatureMatches(existing?.Signature, request.Signature))
    {
      return Results.Ok(currValue);
    }

    // only increment existing values, newly created (0 or seed) are left as-is
    var newValue = existing == null ? currValue : currValue + request.Increment;
    await storage.Set(group, name, new CounterValue(newValue, request.Signature));
    return Results.Ok(newValue);
  }

  private static bool SignatureMatches(string? cvSignature, string? bodySignature)
  {
    if (cvSignature == null && bodySignature == null) return true;
    if (cvSignature == null || bodySignature == null) return false;
    return cvSignature == bodySignature;
  }
}
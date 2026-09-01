using CountersApi.Common;
using CountersApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CountersApi;

public static class Operations
{
  public static async Task<IResult> GetCounter([FromRoute] string group, [FromRoute] string name, ICounterStorage storage, [FromHeader(Name = "X-API-Key")] string? apiKey)
  {
    if (!await storage.IsAuthorized(group, apiKey)) return Results.Unauthorized();

    var current = await storage.Get(group, name);
    if (current == null) return Results.NotFound();
    return Results.Ok(new
    {
      current.Value.Value,
      current.Value.Signature
    });
  }

  public static async Task<IResult> ListCounters([FromRoute] string group, ICounterStorage storage, [FromHeader(Name = "X-API-Key")] string? apiKey)
  {
    if (!await storage.IsAuthorized(group, apiKey)) return Results.Unauthorized();

    var items = await storage.List(group);
    return Results.Ok(items);
  }

  public static async Task<IResult> SetCounter([FromRoute] string group, [FromRoute] string name, [FromBody] CounterRequest request, ICounterStorage storage, [FromHeader(Name = "X-API-Key")] string? apiKey)
  {
    if (!await storage.IsAuthorized(group, apiKey)) return Results.Unauthorized();

    var actualSignature = HashSignatureIfNeeded(request.Signature);

    if (request.Value != null)
    {
      var result = request.Value.Value;
      await storage.Set(group, name, new CounterValue(request.Value.Value, actualSignature));
      return Results.Ok(result);
    }

    var existing = await storage.Get(group, name);
    var currValue = existing?.Value ?? request.Seed ?? 0;

    if (!string.IsNullOrEmpty(actualSignature) && SignatureMatches(existing?.Signature, actualSignature))
    {
      return Results.Ok(currValue);
    }

    // only increment existing values, newly created (0 or seed) are left as-is
    var newValue = existing == null ? currValue : currValue + request.Increment;
    await storage.Set(group, name, new CounterValue(newValue, actualSignature));
    return Results.Ok(newValue);
  }

  private static string? HashSignatureIfNeeded(string? requestSignature)
  {
    if (string.IsNullOrEmpty(requestSignature)) return requestSignature;
    if (IsSha256Hex(requestSignature)) return requestSignature;

    var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(requestSignature));
    return Convert.ToHexString(bytes).ToLowerInvariant();
  }

  private static bool IsSha256Hex(string value)
  {
    if (value.Length != 64) return false;
    foreach (var c in value)
    {
      if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))) return false;
    }

    return true;
  }

  private static bool SignatureMatches(string? cvSignature, string? bodySignature)
  {
    if (cvSignature == null && bodySignature == null) return true;
    if (cvSignature == null || bodySignature == null) return false;
    return cvSignature == bodySignature;
  }
}
using CountersApi.Common;
using CountersApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CountersApi;

public static class Operations
{
  private const string ApiKeyHeaderName = "X-API-Key";


  public static async Task<IResult> GetCounter(
    ICounterStorage storage,
    [FromRoute] string group,
    [FromRoute] string name,
    [FromHeader(Name = ApiKeyHeaderName)] string? apiKey)
  {
    if (!await storage.IsAuthorized(group, apiKey)) return Results.Unauthorized();

    var current = await storage.Get(group, name);
    return current == null
      ? Results.NotFound()
      : Results.Ok(new CounterStateDto(group, name, current.Value));
  }

  public static async Task<IResult> ListCounters(
    ICounterStorage storage,
    [FromRoute] string group,
    [FromHeader(Name = ApiKeyHeaderName)] string? apiKey)
  {
    if (!await storage.IsAuthorized(group, apiKey)) return Results.Unauthorized();

    var items = await storage.List(group);
    return Results.Ok(items);
  }

  public static async Task<IResult> SetCounter(
    ICounterStorage storage,
    [FromRoute] string group, [FromRoute] string name,
    [FromHeader(Name = ApiKeyHeaderName)] string? apiKey,
    [FromBody] CounterRequest request)
  {
    if (!await storage.IsAuthorized(group, apiKey)) return Results.Unauthorized();

    var actualSignature = HashSignatureIfNeeded(request.Signature);

    if (request.Value != null)
    {
      var result = new CounterValue(request.Value.Value, actualSignature);
      await storage.Set(group, name, result);
      return Results.Ok(new CounterStateDto(group, name, result, true));
    }

    var existing = await storage.Get(group, name);
    var currValue = existing?.Value ?? request.Seed ?? 0;

    if (!string.IsNullOrEmpty(actualSignature) && SignatureMatches(existing?.Signature, actualSignature))
    {
      return Results.Ok(new CounterStateDto(group, name, new CounterValue(currValue, existing?.Signature), false));
    }

    // only increment existing values, newly created (0 or seed) are left as-is
    var newValue = new CounterValue(existing == null ? currValue : currValue + request.Increment, actualSignature);
    await storage.Set(group, name, newValue);
    return Results.Ok(new CounterStateDto(group, name, newValue, true));
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

  private static bool SignatureMatches(string? sig1, string? sig2)
  {
    if (sig1 == null && sig2 == null) return true;
    if (sig1 == null || sig2 == null) return false;
    return sig1 == sig2;
  }
}
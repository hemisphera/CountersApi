using System.Text.Json.Serialization;
using CountersApi.Common;

namespace CountersApi.Models;

[JsonSerializable(typeof(CounterStateDto))]
[JsonSerializable(typeof(CounterRequest))]
[JsonSerializable(typeof(CounterValue))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
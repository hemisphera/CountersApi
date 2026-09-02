using CountersApi;
using CountersApi.Common;
using CountersApi.DynamoDb;
using CountersApi.LocalFile;

var builder = WebApplication.CreateSlimBuilder(args);

ICounterStorage storage;

var tableName = Environment.GetEnvironmentVariable("COUNTERSAPI_TABLE_NAME");
if (!string.IsNullOrWhiteSpace(tableName))
{
  var apiKeyTableName = Environment.GetEnvironmentVariable("COUNTERSAPI_API_KEY_TABLE_NAME");
  storage = new DynamoDbStorage(tableName, apiKeyTableName);
}
else
{
  var storagePath = Environment.GetEnvironmentVariable("COUNTERSAPI_STORAGE_PATH") ?? "_storage";
  storage = new LocalFileStorage(storagePath);
}

builder.Services.AddSingleton(storage);
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

// Use the source-generated JSON serializer context (fast-path, no reflection)
// for the API's DTOs; unknown types fall back to reflection via the resolver chain.
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
  options.SerializerOptions.TypeInfoResolverChain.Insert(0, CountersApi.Models.AppJsonContext.Default));

// When running on Lambda this replaces Kestrel with the Lambda runtime client
// (Amazon.Lambda.RuntimeSupport) and processes Lambda events as ASP.NET Core
// requests. When NOT running on Lambda the call is a no-op and Kestrel is used,
// so the same assembly runs standalone and on Lambda.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

var group = app.MapGroup("/counters");
group.MapGet("/{group}/{name}", Operations.GetCounter).WithName(nameof(Operations.GetCounter));
group.MapGet("/{group}", Operations.ListCounters).WithName(nameof(Operations.ListCounters));
group.MapPost("/{group}/{name}", Operations.SetCounter).WithName(nameof(Operations.SetCounter));

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Using storage: {storage}", storage);


app.Run();
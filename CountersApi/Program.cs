using CountersApi;
using CountersApi.Common;
using CountersApi.DynamoDb;
using CountersApi.LocalFile;

var builder = WebApplication.CreateSlimBuilder(args);

ICounterStorage storage;

var tableName = Environment.GetEnvironmentVariable("COUNTERSAPI_TABLE_NAME");
if (!string.IsNullOrWhiteSpace(tableName))
{
  storage = new DynamoDbStorage(tableName);
}
else
{
  var storagePath = Environment.GetEnvironmentVariable("COUNTERSAPI_STORAGE_PATH") ?? "_storage";
  storage = new LocalFileStorage(storagePath);
}

builder.Services.AddSingleton(storage);
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

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
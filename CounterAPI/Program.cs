using CounterAPI;
using CounterAPI.Common;
using CounterAPI.LocalFile;

var builder = WebApplication.CreateSlimBuilder(args);

//builder.Services.AddOpenApi();
var storagePath = Environment.GetEnvironmentVariable("COUNTERAPI_STORAGE_PATH") ?? "_storage";
builder.Services.AddSingleton<ICounterStorage, LocalFileStorage>(_ => new LocalFileStorage(storagePath));
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
var storage = app.Services.GetRequiredService<ICounterStorage>();
logger.LogInformation("Using storage: {storage}", storage);


app.Run();
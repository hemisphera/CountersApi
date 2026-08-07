using CounterAPI;
using CounterAPI.Common;
using CounterAPI.LocalFile;

var builder = WebApplication.CreateSlimBuilder(args);

//builder.Services.AddOpenApi();
var storagePath = Environment.GetEnvironmentVariable("COUNTERAPI_STORAGE_PATH") ?? @"d:\temp\ctr";
builder.Services.AddTransient<ICounterStorage, LocalFileStorage>(a => new LocalFileStorage(storagePath));
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

var todosApi = app.MapGroup("/counters");
todosApi.MapGet("/{group}/{name}", Operations.HandleGet).WithName("GetCounter");
todosApi.MapGet("/{group}", Operations.HandleList).WithName("ListCounters");
todosApi.MapPost("/{group}/{name}", Operations.HandleSet).WithName("SetCounter");

app.Run();
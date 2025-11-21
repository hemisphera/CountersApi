using CounterAPI;
using CounterAPI.Common;
using CounterAPI.LocalFile;

var builder = WebApplication.CreateSlimBuilder(args);

//builder.Services.AddOpenApi();
builder.Services.AddTransient<ICounterStorage, LocalFileStorage>(a => new LocalFileStorage(@"d:\temp\ctr"));
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
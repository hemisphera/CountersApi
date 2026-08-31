using Amazon.CDK;

namespace CountersApi.Deploy;

sealed class Program
{
  public static void Main(string[] args)
  {
    Globals.EnvironmentName = System.Environment.GetEnvironmentVariable("ENV") ?? "dev";

    var app = new App();
    var stack = new CountersApiStack(app, "CountersApiStack", new StackProps
    {
    });

    app.Synth();
  }
}
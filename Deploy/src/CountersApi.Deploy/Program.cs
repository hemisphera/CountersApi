using Amazon.CDK;

namespace CountersApi.Deploy;

sealed class Program
{
  public static void Main(string[] args)
  {
    Globals.EnvironmentName = System.Environment.GetEnvironmentVariable("ENV") ?? "dev";

    var app = new App();

    // Deploy order is driven by cross-stack references: the repo stack is
    // deployed before the app stack (which imports the repo URI).
    var repoStack = new CountersApiRepoStack(app, "CountersApiRepoStack", new StackProps
    {
    });
    _ = new CountersApiStack(app, "CountersApiStack", new StackProps
    {
    }, repoStack.Repository);

    app.Synth();
  }
}
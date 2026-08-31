using Amazon.CDK;

namespace CountersApiDeploy;

sealed class Program
{
  public static void Main(string[] args)
  {
    Globals.EnvironmentName = System.Environment.GetEnvironmentVariable("ENV") ?? "dev";

    var app = new App();
    var stack = new CountersApiDeployStack(app, "CountersApiDeployStack", new StackProps
    {
    });

    app.Synth();
  }
}
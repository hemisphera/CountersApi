using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Constructs;

namespace CountersApi.Deploy;

/// <summary>
///   Owns the ECR repository that holds the API's container image. CI builds the
///   Linux image and pushes it here; the app stack deploys Lambda pointing at a tag.
///   Split into its own stack so the repo can exist (and CI can push to it) before
///   the Lambda function is created.
/// </summary>
public class CountersApiRepoStack : Stack
{
  internal IRepository Repository { get; }

  internal CountersApiRepoStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
  {
    Repository = new Repository(this, "Repo", new RepositoryProps
    {
      RepositoryName = $"{Globals.Name}-{Globals.EnvironmentName}-repo",
      // CI re-pushes the mutable :latest tag on every build.
      ImageTagMutability = TagMutability.MUTABLE,
      ImageScanOnPush = true,
      RemovalPolicy = RemovalPolicy.DESTROY,
      // Let the repo be deleted even if it still holds images (dev teardown).
      EmptyOnDelete = true,
      LifecycleRules =
      [
        new LifecycleRule
        {
          Description = "Keep only the 10 most recent images",
          MaxImageCount = 5
        }
      ]
    });

    _ = new CfnOutput(this, "RepositoryName", new CfnOutputProps
    {
      Value = Repository.RepositoryName,
      Description = "ECR repository holding the API image"
    });

    _ = new CfnOutput(this, "RepositoryUri", new CfnOutputProps
    {
      Value = Repository.RepositoryUri,
      Description = "ECR repository URI (push images here)"
    });
  }
}
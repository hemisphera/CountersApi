using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.SSM;
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

  /// <summary>
  ///   SSM parameter holding the ECR repository name; CI (GitHub Actions) reads
  ///   the image repository from here instead of polling stack outputs.
  /// </summary>
  internal static string RepositoryNameParameterPath => $"/{Globals.Name}/ecr/repository-name";

  internal CountersApiRepoStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
  {
    Repository = new Repository(this, "Repo", new RepositoryProps
    {
      RepositoryName = $"{Globals.Name}-repo",
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

    // Written on every deploy of this stack, so CI always sees the current
    // repo name even if Globals.Name changes and the repository is recreated.
    _ = new StringParameter(this, "RepositoryNameParameter", new StringParameterProps
    {
      ParameterName = RepositoryNameParameterPath,
      StringValue = Repository.RepositoryName,
      Description = "ECR repository holding the API container image (consumed by CI)"
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
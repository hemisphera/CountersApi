using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace CountersApi.Deploy;

public class Api : Construct
{
  internal Api(Construct scope, Storage storage, IRepository repository) : base(scope, "Api")
  {
    // Reference an image CI has already pushed to ECR. No local Docker build
    // happens at synth/deploy time. The image's ENTRYPOINT runs
    // `dotnet CountersApi.dll`; AddAWSLambdaHosting self-bootstraps the Lambda
    // runtime (Amazon.Lambda.RuntimeSupport) to process Lambda events as
    // ASP.NET Core requests, so no Lambda Web Adapter is needed.
    //
    // The tag defaults to "latest"; CI passes a unique sha tag via
    // `cdk deploy -c imageTag=...`. With a mutable tag alone CloudFormation
    // sees no template change on image-only updates and would skip the Lambda,
    // so a unique tag forces a real function-code refresh each push.
    var imageTag = Node.TryGetContext("imageTag") as string ?? "latest";

    var code = DockerImageCode.FromEcr(repository, new EcrImageCodeProps
    {
      TagOrDigest = imageTag
    });

    var func = new DockerImageFunction(this, "Function", new DockerImageFunctionProps
    {
      Code = code,
      // .NET cold start is acceptable here; 512MB keeps per-invocation cost low.
      MemorySize = 512,
      Timeout = Duration.Seconds(30),
      Environment = new Dictionary<string, string>
      {
        ["COUNTERSAPI_TABLE_NAME"] = storage.Table.TableName,
        ["COUNTERSAPI_API_KEY_TABLE_NAME"] = storage.ApiKeyTable.TableName
      }
    });

    storage.Table.GrantReadWriteData(func);
    storage.ApiKeyTable.GrantReadData(func);

    // Function URL = public HTTPS endpoint with no API Gateway cost (scales to zero).
    // NONE matches the previous public ALB; switch to AWS_IAM for authenticated access.
    var url = func.AddFunctionUrl(new FunctionUrlOptions
    {
      AuthType = FunctionUrlAuthType.NONE
    });

    new CfnOutput(this, "Endpoint", new CfnOutputProps
    {
      Value = url.Url,
      Description = "URL of the Counter API (Lambda Function URL)"
    });
  }
}
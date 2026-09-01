using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Constructs;

namespace CountersApi.Deploy;

public class CountersApiStack : Stack
{
  internal CountersApiStack(Construct scope, string id, IStackProps props, IRepository repository) : base(scope, id, props)
  {
    var storage = new Storage(this);
    _ = new Api(this, storage, repository);
  }
}
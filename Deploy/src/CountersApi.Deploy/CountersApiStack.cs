using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace CountersApi.Deploy;

public class CountersApiStack : Stack
{
  internal CountersApiStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
  {
    var storage = new Storage(this);
    new Api(this, storage);
  }
}
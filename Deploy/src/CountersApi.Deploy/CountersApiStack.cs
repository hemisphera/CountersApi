using Amazon.CDK;
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
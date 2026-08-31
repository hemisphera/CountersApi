using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Constructs;

namespace CountersApi.Deploy;

public class Api : Construct
{
  internal Api(Construct scope, Storage storage) : base(scope, "CountersApi_Api")
  {
    var vpc = new Vpc(this, "CountersApi_Vpc", new VpcProps
    {
      MaxAzs = 2
    });

    var service = new ApplicationLoadBalancedFargateService(this, "CountersApi_Service", new ApplicationLoadBalancedFargateServiceProps
    {
      Vpc = vpc,
      PublicLoadBalancer = true,
      DesiredCount = 1,
      AssignPublicIp = true,
      TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
      {
        Image = ContainerImage.FromRegistry("ahedfour/countersapi"),
        ContainerPort = 8080,
        Environment = new Dictionary<string, string>
        {
          ["TABLE_NAME"] = storage.Table.TableName
        }
      }
    });

    storage.Table.GrantReadWriteData(service.TaskDefinition.TaskRole);

    new CfnOutput(this, "CountersApi_Endpoint", new CfnOutputProps
    {
      Value = $"http://{service.LoadBalancer.LoadBalancerDnsName}",
      Description = "URL of the Counter API"
    });
  }
}
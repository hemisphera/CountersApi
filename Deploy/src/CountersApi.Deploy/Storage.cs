using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace CountersApi.Deploy;

public class Storage : Construct
{
  internal TableV2 Table { get; }

  internal Storage(Construct scope) : base(scope, "CountersApi_Storage")
  {
    Table = new TableV2(this, "CountersApi_Table", new TablePropsV2
    {
      RemovalPolicy = RemovalPolicy.DESTROY,
      TableName = $"{Globals.Name}-{Globals.EnvironmentName}-{Globals.InternalId:D}",
      PartitionKey = new Attribute
      {
        Name = "Group",
        Type = AttributeType.STRING
      },
      SortKey = new Attribute
      {
        Name = "Name",
        Type = AttributeType.STRING
      }
    });

    new CfnOutput(this, "CountersApi_Table_Name", new CfnOutputProps
    {
      Value = Table.TableName,
      Description = "Name of the table"
    });
  }
}
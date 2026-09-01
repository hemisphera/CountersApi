using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace CountersApi.Deploy;

public class Storage : Construct
{
  internal TableV2 Table { get; }
  internal TableV2 ApiKeyTable { get; }

  internal Storage(Construct scope) : base(scope, "Storage")
  {
    Table = new TableV2(this, "Table", new TablePropsV2
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

    ApiKeyTable = new TableV2(this, "ApiKeyTable", new TablePropsV2
    {
      RemovalPolicy = RemovalPolicy.DESTROY,
      TableName = $"{Globals.Name}-{Globals.EnvironmentName}-{Globals.InternalId:D}",
      PartitionKey = new Attribute
      {
        Name = "Key",
        Type = AttributeType.STRING
      }
    });

    _ = new CfnOutput(this, $"{nameof(Table)}Name", new CfnOutputProps
    {
      Value = Table.TableName,
      Description = "Name of the table"
    });
    _ = new CfnOutput(this, $"{nameof(ApiKeyTable)}Name", new CfnOutputProps
    {
      Value = ApiKeyTable.TableName,
      Description = "Name of the API key table"
    });
  }
}
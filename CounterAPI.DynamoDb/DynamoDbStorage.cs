using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CounterAPI.Common;

namespace CounterAPI.DynamoDb;

/// <summary>
///   Stores counters in a DynamoDB table. The table is expected to already
///   exist with a partition key named "Group" (String) and a sort key named
///   "Name" (String). Counter values are stored as a number attribute "Value"
///   and the optional signature as a string attribute "Signature".
/// </summary>
public class DynamoDbStorage : ICounterStorage
{
  private const string PartitionKeyName = "Group";
  private const string SortKeyName = "Name";
  private const string ValueAttributeName = "Value";
  private const string SignatureAttributeName = "Signature";

  private readonly IAmazonDynamoDB _client;
  private readonly string _tableName;


  /// <summary>
  ///   Creates a new <see cref="DynamoDbStorage" /> using the default AWS
  ///   credential chain, which picks up ECS task role credentials when running
  ///   inside ECS, and the default AWS region from the environment.
  /// </summary>
  public DynamoDbStorage(string tableName)
    : this(new AmazonDynamoDBClient(), tableName)
  {
  }

  public DynamoDbStorage(IAmazonDynamoDB client, string tableName)
  {
    _client = client;
    _tableName = tableName;
  }


  public async Task<CounterValue?> Get(string group, string name)
  {
    var response = await _client.GetItemAsync(new GetItemRequest
    {
      TableName = _tableName,
      Key = BuildKey(group, name)
    });

    if (!response.IsItemSet) return null;

    var value = 0L;
    if (response.Item.TryGetValue(ValueAttributeName, out var valueAttr) && valueAttr.N is { } number)
    {
      value = long.Parse(number);
    }

    string? signature = null;
    if (response.Item.TryGetValue(SignatureAttributeName, out var sigAttr) && sigAttr.S is { } sig)
    {
      signature = sig.NullIfWhitespace();
    }

    return new CounterValue(value, signature);
  }

  public async Task Set(string group, string name, CounterValue value)
  {
    var item = BuildKey(group, name);
    item[ValueAttributeName] = new AttributeValue { N = value.Value.ToString() };
    if (!string.IsNullOrEmpty(value.Signature))
    {
      item[SignatureAttributeName] = new AttributeValue { S = value.Signature };
    }

    await _client.PutItemAsync(new PutItemRequest
    {
      TableName = _tableName,
      Item = item
    });
  }

  public async Task<IEnumerable<string>> List(string group)
  {
    var response = await _client.QueryAsync(new QueryRequest
    {
      TableName = _tableName,
      KeyConditionExpression = "#pk = :g",
      ExpressionAttributeNames = new Dictionary<string, string>
      {
        ["#pk"] = PartitionKeyName,
        ["#sk"] = SortKeyName
      },
      ExpressionAttributeValues = new Dictionary<string, AttributeValue>
      {
        [":g"] = new AttributeValue { S = group }
      },
      ProjectionExpression = "#sk"
    });

    return response.Items
      .Where(item => item.TryGetValue(SortKeyName, out var nameAttr) && nameAttr.S is not null)
      .Select(item => item[SortKeyName].S!);
  }


  private static Dictionary<string, AttributeValue> BuildKey(string group, string name)
  {
    return new Dictionary<string, AttributeValue>
    {
      [PartitionKeyName] = new AttributeValue { S = group },
      [SortKeyName] = new AttributeValue { S = name }
    };
  }


  public override string ToString()
  {
    return $"DynamoDbStorage: {_tableName}";
  }
}
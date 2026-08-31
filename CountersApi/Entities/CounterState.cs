using System.ComponentModel.DataAnnotations.Schema;
using Hsp.Azure.Table.Orm;

namespace CountersApi.Entities;

[Table("Counter")]
public class CounterState
{
  [PartitionKey]
  public string Group { get; set; } = string.Empty;

  [RowKey]
  public string Name { get; set; } = string.Empty;

  [Column]
  public int Value { get; set; }

  [Column]
  public string Signature { get; set; } = string.Empty;
}
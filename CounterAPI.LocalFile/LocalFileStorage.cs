using CounterAPI.Common;

namespace CounterAPI.LocalFile;

public class LocalFileStorage : ICounterStorage
{
  private readonly string _root;


  public LocalFileStorage(string root)
  {
    _root = new DirectoryInfo(root).FullName;
  }


  public async Task<CounterValue?> Get(string group, string name)
  {
    var file = GetCounterFile(group, name);
    if (!file.Exists) return null;
    var contents = await File.ReadAllLinesAsync(file.FullName);
    return new CounterValue(
      contents.Length > 0 ? long.Parse(contents[0]) : 0,
      (contents.Length > 1 ? contents[1] : string.Empty).NullIfWhitespace()
    );
  }

  public async Task Set(string group, string name, CounterValue value)
  {
    var file = GetCounterFile(group, name);
    file.Directory?.Create();
    await File.WriteAllLinesAsync(
      file.FullName,
      [
        value.Value.ToString(),
        value.Signature ?? string.Empty
      ]);
  }

  public async Task<IEnumerable<string>> List(string group)
  {
    var groupFolder = GetGroupFolder(group);
    var items = groupFolder.Exists
      ? groupFolder.GetFileSystemInfos().Where(fsi => fsi is DirectoryInfo).Select(dir => dir.Name)
      : [];
    return await ValueTask.FromResult(items.Select(StringExtensions.Sanitize));
  }

  private DirectoryInfo GetGroupFolder(string group)
  {
    return new DirectoryInfo(Path.Combine(_root, group.Sanitize()));
  }

  private FileInfo GetCounterFile(string group, string name)
  {
    var folder = GetGroupFolder(group);
    return new FileInfo(Path.Combine(folder.FullName, name.Sanitize()));
  }


  public override string ToString()
  {
    return $"LocalFileStorage: {_root}";
  }
}
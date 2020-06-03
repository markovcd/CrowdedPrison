using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace CrowdedPrison.Common
{
  internal class FileJsonSerializer : IFileSerializer
  {
    public async Task<T> DeserializeAsync<T>(string path)
    {
      try
      {
        await using var fileStream = File.OpenRead(path);
        using var jsonTextReader = new JsonTextReader(new StreamReader(fileStream));
        var serializer = new JsonSerializer();
        return serializer.Deserialize<T>(jsonTextReader);
      }
      catch
      {
        return default;
      }
    }

    public async Task<bool> SerializeAsync<T>(T obj, string path)
    {
      await using var fileStream = File.Create(path);
      try
      {
        using var jsonWriter = new JsonTextWriter(new StreamWriter(fileStream));
        var serializer = new JsonSerializer();
        serializer.Serialize(jsonWriter, obj);
        await jsonWriter.FlushAsync();
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
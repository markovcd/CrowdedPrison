using System.Threading.Tasks;

namespace CrowdedPrison.Common
{
  public interface IFileSerializer
  {
    Task<bool> SerializeAsync<T>(T obj, string path);
    Task<T> DeserializeAsync<T>(string path);
  }
}
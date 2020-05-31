using System;

namespace CrowdedPrison.Common
{
  public class OutputData
  {
    public string Data { get; }
    public bool IsError { get; }
    public DateTime Timestamp { get; }

    public OutputData(string data, bool isError)
    {
      Data = data;
      IsError = isError;
      Timestamp = DateTime.Now;
    }
  }
}
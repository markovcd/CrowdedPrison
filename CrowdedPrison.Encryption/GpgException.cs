using System;

namespace CrowdedPrison.Encryption
{
  public class GpgException : Exception
  {
    public GpgException(string message) : base(message) { }
  }
}
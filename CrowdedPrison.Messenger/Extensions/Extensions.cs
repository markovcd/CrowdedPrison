using System;
using System.Collections.Generic;
using System.Text;

namespace CrowdedPrison.Messenger.Extensions
{
  public static class Extensions
  {
    public static DateTime FromUnixEpoch(this string s, bool milliseconds = false)
    {
      if (s == null) return default;

      return long.TryParse(s, out var l)
        ? l.FromUnixEpoch(milliseconds)
        : default;
    }

    public static DateTime FromUnixEpoch(this long l, bool milliseconds = false)
    {
      var dt = milliseconds 
        ? DateTimeOffset.FromUnixTimeMilliseconds(l) 
        : DateTimeOffset.FromUnixTimeSeconds(l);

      return dt.DateTime.ToLocalTime();
    }
  }
}

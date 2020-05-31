using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using fbchat_sharp.API;
using Newtonsoft.Json;

namespace CrowdedPrison.Messenger
{
  internal class BaseClient : MessengerClient
  {
    public Action<FB_Event> FbEventCallback { get; set; }
    public Func<Task> DeleteCookiesCallback { get; set; }
    public Func<Task<Dictionary<string, List<Cookie>>>> ReadCookiesFromDiskCallback { get; set; }
    public Func<Dictionary<string, List<Cookie>>, Task> WriteCookiesToDiskCallback { get; set; }

    protected override async Task OnEvent(FB_Event ev)
    {
      FbEventCallback?.Invoke(ev);
    
      await Task.Yield();
    }

    protected override async Task DeleteCookiesAsync()
    {
      if (DeleteCookiesCallback != null)
        await DeleteCookiesCallback();
    }

    protected override async Task<Dictionary<string, List<Cookie>>> ReadCookiesFromDiskAsync()
    {
      if (DeleteCookiesCallback != null)
        return await ReadCookiesFromDiskCallback();

      return null;
    }

    protected override async Task WriteCookiesToDiskAsync(Dictionary<string, List<Cookie>> cookieJar)
    {
      if (WriteCookiesToDiskCallback != null)
        await WriteCookiesToDiskCallback(cookieJar);    
    }
  }
}
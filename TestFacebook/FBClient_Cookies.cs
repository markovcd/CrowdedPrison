using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using fbchat_sharp.API;
using Newtonsoft.Json;

namespace TestFacebook
{
  public class FBClient_Cookies : MessengerClient
  {
    private static readonly string appName = "FBChat-Sharp";
    private static readonly string sessionFile = "SESSION_COOKIES_core.dat";

    public Action<FB_Event> FbEventCallback { get; set; }

    protected override async Task OnEvent(FB_Event ev)
    {
      FbEventCallback?.Invoke(ev);
    
      await Task.Yield();
    }

    protected override async Task DeleteCookiesAsync()
    {
      try
      {
        await Task.Yield();
        var file = Path.Combine(UserDataFolder, sessionFile);
        File.Delete(file);
      }
      catch (Exception ex)
      {
        this.Log(ex.ToString());
      }
    }

    protected override async Task<Dictionary<string, List<Cookie>>> ReadCookiesFromDiskAsync()
    {
      try
      {
        var file = Path.Combine(UserDataFolder, sessionFile);
        using var fileStream = File.OpenRead(file);
        await Task.Yield();
        using var jsonTextReader = new JsonTextReader(new StreamReader(fileStream));
        var serializer = new JsonSerializer();
        return serializer.Deserialize<Dictionary<string, List<Cookie>>>(jsonTextReader);
      }
      catch (Exception ex)
      {
        this.Log(string.Format("Problem reading cookies from disk: {0}", ex.ToString()));
        return null;
      }
    }

    protected override async Task WriteCookiesToDiskAsync(Dictionary<string, List<Cookie>> cookieJar)
    {
      var file = Path.Combine(UserDataFolder, sessionFile);

      using var fileStream = File.Create(file);
      try
      {
        using var jsonWriter = new JsonTextWriter(new StreamWriter(fileStream));
        JsonSerializer serializer = new JsonSerializer();
        serializer.Serialize(jsonWriter, cookieJar);
        await jsonWriter.FlushAsync();
      }
      catch (Exception ex)
      {
        Log(string.Format("Problem writing cookies to disk: {0}", ex.ToString()));
      }
    }

    /// <summary>
    /// Get the current user data folder
    /// </summary>
    private static string UserDataFolder
    {
      get
      {
        string folderBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dir = Path.Combine(folderBase, appName.ToUpper());
        Directory.CreateDirectory(dir);
        return dir;
      }
    }
  }
}
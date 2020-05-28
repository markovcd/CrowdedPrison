﻿using fbchat_sharp.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestFacebook
{
  public class FBClient_Cookies : MessengerClient
  {
    private static readonly string appName = "FBChat-Sharp";
    private static readonly string sessionFile = "SESSION_COOKIES_core.dat";

    public FBClient_Cookies()
    {
      On2FACodeCallback = get2FACode;
    }

    private async Task<string> get2FACode()
    {
      await Task.Yield();
      Console.WriteLine("Insert 2FA code:");
      return Console.ReadLine();
    }


    protected override async Task OnEvent(FB_Event ev)
    {
      switch (ev)
      {
        case FB_MessageEvent t1:
          Console.WriteLine(string.Format("Got new message from {0}: {1}", t1.author, t1.message));
          break;
        default:
          Console.WriteLine(string.Format("Something happened: {0}", ev.ToString()));
          break;
      }
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
        return CheckDir(dir);
      }
    }

    /// <summary>
    /// Check the specified folder, and create if it doesn't exist.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    private static string CheckDir(string dir)
    {
      Directory.CreateDirectory(dir);
      return dir;
    }
  }
  class Class1
  {


    public async void Login()
    {
      var messenger = new FBClient_Cookies();
      var s = await messenger.DoLogin("markovcd@gmail.com", "oyM7kIE4JVd6");
    }
  }
}

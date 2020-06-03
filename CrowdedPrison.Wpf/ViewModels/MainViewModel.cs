﻿using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Events;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CrowdedPrison.Messenger.Encryption.Events;
using CrowdedPrison.Encryption;
using Prism.Regions;
using CrowdedPrison.Wpf.Services;
using CrowdedPrison.Common;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class MainViewModel : BindableBase, INavigationAware
  {
    private readonly IMessenger messenger;
    private readonly IGpgMessenger gpgMessenger;
    private readonly IMainDialogService dialogService;
    private readonly AppConfiguration configuration;
    private readonly IGpgDownloader downloader;
    private readonly IShellService shellService;
    private readonly IFileSystem fileSystem;
    private readonly IFileSerializer serializer;
    private readonly ITwoWayEncryption encryption;

    public ICommand ConnectCommand { get; }

    public MainViewModel(IMessenger messenger, IGpgMessenger gpgMessenger, IMainDialogService dialogService,
      AppConfiguration configuration, IGpgDownloader downloader, IShellService shellService, IFileSystem fileSystem,
      IFileSerializer serializer, ITwoWayEncryption encryption)
    {
      this.messenger = messenger;
      this.gpgMessenger = gpgMessenger;
      this.dialogService = dialogService;
      this.configuration = configuration;
      this.downloader = downloader;
      this.shellService = shellService;
      this.fileSystem = fileSystem;
      this.serializer = serializer;
      this.encryption = encryption;

      ConnectCommand = new DelegateCommand(() => ConnectAsync());

      messenger.ConnectionStateChanged += Messenger_ConnectionStateChanged;
      messenger.MessageReceived += Messenger_MessageReceived;
      messenger.MessagesDelivered += Messenger_MessagesDelivered;
      messenger.TwoFactorRequested += Messenger_TwoFactorRequested;
      messenger.UserLoginRequested += Messenger_UserLoginRequested;
      messenger.MessageUnsent += Messenger_MessageUnsent;
      messenger.Typing += Messenger_Typing;
      gpgMessenger.EncryptedMessageReceived += GpgMessenger_EncryptedMessageReceived;
      shellService.Cloing += ShellService_Cloing;
    }


    private async Task SaveSettingsAsync()
    {
      await serializer.SerializeAsync(configuration, configuration.SettingsFilePath);
    }

    private async Task<bool> DownloadGpgAsync()
    {
      if (configuration.GpgPath == null)
        configuration.GpgPath = await dialogService.ShowDownloadGpgDialogAsync();
      else 
        return true;

      if (configuration.GpgPath == null)
      {
        await dialogService.ShowMessageDialogAsync("Error during GnuPG installation. Application will be closed.");
        shellService.Close();
        return false;
      }

      await dialogService.ShowMessageDialogAsync("GnuPG installation was successful.");
      return true;
    }

    private async Task<bool> CheckGpgPasswordAsync()
    {
      if (!string.IsNullOrEmpty(configuration.GpgPasswordHash)
        && encryption.TryDecrypt(configuration.GpgPasswordHash, out _))
      {
        return true;
      }

      while (true)
      {
        var password = await dialogService.ShowPasswordDialogAsync("GnuPG Password");
        if (password == null)
        {
          await dialogService.ShowMessageDialogAsync($"{GlobalConstants.AppName} needs GnuPG password to work. Application will be closed.");
          shellService.Close();
        }

        var confirmPassword = await dialogService.ShowPasswordDialogAsync("Confirm GnuPG Password");
        if (confirmPassword != password)
        {
          await dialogService.ShowMessageDialogAsync($"Passwords differ. Enter them again.");
          continue;
        }

        configuration.GpgPasswordHash = encryption.Encrypt(password);
        return true;
      }
    }

    private async Task ConnectAsync()
    {
      bool success;

      try
      {
        ShowSpinner();
        success = await messenger.LoginAsync();
      }
      catch (OperationCanceledException)
      {
        success = true;
      }
     
      finally
      {
        await HideSpinnerAsync();
      }

      if (!success) await dialogService.ShowMessageDialogAsync("Login to Facebook failed. Make sure you entered correct password or two factor code.");
    }

    private void ShowSpinner()
    {
      dialogService.ShowSpinnerDialogAsync("Logging in...");
    }

    private async Task HideSpinnerAsync()
    {
      await dialogService.HideSpinnerDialogAsync();
    }

    private async Task LoadSettingsAsync()
    {
      configuration.HomeDir = fileSystem.GetHomeDirectoryPath(GlobalConstants.AppName);
      configuration.GpgPath = downloader.GetGpgPath();
      configuration.SettingsFilePath = fileSystem.CombinePaths(configuration.HomeDir, "settings.dat");

      var deserialized = await serializer.DeserializeAsync<AppConfiguration>(configuration.SettingsFilePath);
      configuration.GpgPasswordHash = deserialized?.GpgPasswordHash;
      configuration.MessengerEmail = deserialized?.MessengerEmail;
    }

    private async void ShellService_Cloing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      await SaveSettingsAsync();
    }

    private void GpgMessenger_EncryptedMessageReceived(object sender, EncryptedMessageReceivedEventArgs e)
    {
      Debug.WriteLine(e.DecryptedText);
    }

    private void Messenger_Typing(object sender, TypingEventArgs e)
    {
      
    }

    private void Messenger_MessageUnsent(object sender, MessageUnsentEventArgs e)
    {
      
    }

    private async Task Messenger_UserLoginRequested(object sender, UserLoginEventArgs e)
    {
      await HideSpinnerAsync();
      (e.Email, e.Password) = await dialogService.ShowLoginDialogAsync(configuration.MessengerEmail);
      e.IsCancelled = e.Email == null;

      if (!e.IsCancelled) configuration.MessengerEmail = e.Email;

      ShowSpinner();
    }

    private async Task Messenger_TwoFactorRequested(object sender, TwoFactorEventArgs e)
    {
      await HideSpinnerAsync();
      e.TwoFactorCode = await dialogService.ShowInputDialogAsync("Two factor code");
      e.IsCancelled = e.TwoFactorCode == null;
      ShowSpinner();
    }

    private void Messenger_MessagesDelivered(object sender, MessagesDeliveredEventArgs e)
    {
      
    }

    private void Messenger_MessageReceived(object sender, MessageReceivedEventArgs e)
    {

    }

    private void Messenger_ConnectionStateChanged(object sender, ConnectionStateEventArgs e)
    {
      Debug.WriteLine($"{e.State} {e.Reason}");
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
      await LoadSettingsAsync();
      if (await CheckGpgPasswordAsync() && await DownloadGpgAsync()) await ConnectAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {

    }
  }
}

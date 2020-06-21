using CrowdedPrison.Messenger;
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
using CrowdedPrison.Core.Services;
using CrowdedPrison.Common;
using System.Collections.ObjectModel;
using System.Linq;
using CrowdedPrison.Messenger.Entities;
using Prism.Events;

namespace CrowdedPrison.Core.ViewModels
{
  public class MessageReceivedEvent : PubSubEvent<MessageReceivedEventArgs> { }

  public class MainViewModel : BindableBase, INavigationAware
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
    private readonly Func<IUserViewModel> userViewModelFactory;
    private readonly IEventAggregator events;
    private IUserViewModel selectedUser;

    public ObservableCollection<IUserViewModel> Users { get; } = new ObservableCollection<IUserViewModel>();
    public IUserViewModel SelectedUser
    {
      get => selectedUser;
      set 
      {
        var oldSelectedUser = selectedUser;
        if (SetProperty(ref selectedUser, value))
        {
          oldSelectedUser?.ChatClosed();
          value?.ChatOpened();
        }
      }
    }

    public MainViewModel(IMessenger messenger, IGpgMessenger gpgMessenger, IMainDialogService dialogService,
      AppConfiguration configuration, IGpgDownloader downloader, IShellService shellService, IFileSystem fileSystem,
      IFileSerializer serializer, ITwoWayEncryption encryption, Func<IUserViewModel> userViewModelFactory, IEventAggregator events)
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
      this.userViewModelFactory = userViewModelFactory;
      this.events = events;
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

    private async void Init()
    {
      await LoadSettingsAsync();
      if (await CheckGpgPasswordAsync() && await DownloadGpgAsync())
      {
        if (await ConnectAsync())
        {
          if (!await gpgMessenger.IsPrivateKeyPresentAsync())
          {
            if (!await gpgMessenger.GeneratePrivateKeyAsync())
            {
              await dialogService.ShowMessageDialogAsync("Failed to generate private key. Application will be closed.");
              shellService.Close();
            }
          }
        }
      }
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

    private async Task<bool> ConnectAsync()
    {
      bool success;
      var cancelled = false;

      try
      {
        ShowSpinner();
        success = await messenger.LoginAsync();
      }
      catch (OperationCanceledException)
      {
        cancelled = true;
        success = false;
      }
     
      finally
      {
        await HideSpinnerAsync();
      }

      if (!success && !cancelled) await dialogService.ShowMessageDialogAsync("Login to Facebook failed. Make sure you entered correct password or two factor code.");

      if (success)
      {
        var userVms = messenger.Users.Select(CreateUserViewModel);
        Users.Clear();
        Users.AddRange(userVms);
      }

      return success;    
    }

    private IUserViewModel CreateUserViewModel(MessengerUser user)
    {
      var vm = userViewModelFactory();
      vm.User = user;
      return vm;
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
      events.GetEvent<MessageReceivedEvent>().Publish(e);
    }

    private void Messenger_ConnectionStateChanged(object sender, ConnectionStateEventArgs e)
    {
      Debug.WriteLine($"{e.State} {e.Reason}");
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      Init();
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

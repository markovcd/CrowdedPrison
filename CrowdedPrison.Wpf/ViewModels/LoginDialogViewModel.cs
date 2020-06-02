using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class LoginDialogViewModel : BaseDialogViewModel<(string email, string password)>
  {
    private string email;

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public string Email
    {
      get => email;
      set => SetProperty(ref email, value);
    }

    public LoginDialogViewModel()
    {
      OkCommand = new DelegateCommand<PasswordBox>(Ok);
      CancelCommand = new DelegateCommand(Cancel);
    }

    private void Ok(PasswordBox passwordBox)
    {
      SetResult((Email, passwordBox.Password));
    }

    private void Cancel()
    {
      SetResult((null, null));
    }
  }
}
using System.Windows.Input;
using Prism.Commands;

namespace CrowdedPrison.Wpf.ViewModels
{
  public class LoginDialogViewModel : BaseDialogViewModel<bool>
  {
    private string welcomeText;
    private string email;
    private string password;

    public ICommand OkCommand => new DelegateCommand(() => SetResult(true));

    public string WelcomeText
    {
      get => welcomeText;
      set => SetProperty(ref welcomeText, value);
    }

    public string Email
    {
      get => email;
      set => SetProperty(ref email, value);
    }

    public string Password
    {
      get => password;
      set => SetProperty(ref password, value);
    }
  }
}
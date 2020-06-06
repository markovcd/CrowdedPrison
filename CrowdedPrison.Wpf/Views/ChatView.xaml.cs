using System.Windows;
using System.Windows.Threading;

namespace CrowdedPrison.Wpf.Views
{
  public partial class ChatView 
  {
    public ChatView()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      Application.Current.Dispatcher.BeginInvoke(() => messageText.Clear(), DispatcherPriority.ApplicationIdle);
    }
  }
}

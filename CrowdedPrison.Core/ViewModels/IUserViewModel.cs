using CrowdedPrison.Messenger.Entities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CrowdedPrison.Core.ViewModels
{
  public interface IUserViewModel
  {
    MessengerUser User { get; set; }
    ObservableCollection<MessengerMessage> Messages { get; }
    Task RefreshMessagesAsync(int limit = 20);
    void ClearMessages();
  }
}

using CrowdedPrison.Messenger.Entities;
using System.Collections.ObjectModel;

namespace CrowdedPrison.Core.ViewModels
{
  public interface IUserViewModel
  {
    MessengerUser User { get; set; }
    ObservableCollection<MessengerMessage> Messages { get; }
  }
}

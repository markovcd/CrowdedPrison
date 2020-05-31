using fbchat_sharp.API;

namespace CrowdedPrison.Messenger.Entities
{
  public class MessengerThread
  {
    public int MessageCount { get; }
    public string Name { get; }
    public string Id { get; }
    public string PhotoUrl { get; }

    internal MessengerThread(FB_Thread thread)
    {
      MessageCount = thread.message_count;
      Name = thread.name;
      Id = thread.uid;
      PhotoUrl = thread.photo?.url;
    }
  }
}

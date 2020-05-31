using fbchat_sharp.API;

namespace CrowdedPrison.Messenger.Entities
{
  public class MessengerMessage
  {
    public string Text { get; }
    public string ThreadId { get; }
    public string Author { get; }
    public string Timestamp { get; }
    public string Id { get; }
    public bool IsRead { get; }
    public bool IsUnsent { get; }

    internal MessengerMessage(FB_Message message)
    {
      Text = message.text;
      ThreadId = message.thread_id;
      Author = message.author;
      Timestamp = message.timestamp;
      Id = message.uid;
      IsRead = message.is_read;
      IsUnsent = message.unsent;
    }
  }
}
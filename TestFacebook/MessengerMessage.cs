using fbchat_sharp.API;

namespace TestFacebook
{
  public class MessengerMessage
  {
    public string Text { get; }

    internal MessengerMessage(FB_Message message)
    {
      Text = message.text;
    }
  }
}
﻿using TestFacebook.Entities;

namespace TestFacebook.Events
{
  public class TypingEventArgs
  {
    public MessengerUser User { get; }
    public MessengerThread Thread { get; }
    public bool IsTyping { get; }

    public TypingEventArgs(MessengerUser user, MessengerThread thread, bool isTyping)
    {
      User = user;
      Thread = thread;
      IsTyping = isTyping;
    }
  }
}
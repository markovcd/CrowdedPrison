using System;
using fbchat_sharp.API;

namespace CrowdedPrison.Messenger.Entities
{
  public class MessengerUser
  {
    public string Id { get; }
    public string Name { get; }
    public bool IsFriend { get; }
    public string Url { get; }
    public string PhotoUrl { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Nickname { get; }
    public string OwnNickname { get; }
    public string Gender { get; }
    public bool IsActive { get; set; }
    public DateTime LastActive { get; set; }

    internal MessengerUser(FB_User user)
    {
      Id = user.uid;
      Name = user.name;
      IsFriend = user.is_friend;
      Url = user.url;
      PhotoUrl = user.photo?.url;
      FirstName = user.first_name;
      LastName = user.last_name;
      Nickname = user.nickname;
      OwnNickname = user.own_nickname;
      Gender = user.gender;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}

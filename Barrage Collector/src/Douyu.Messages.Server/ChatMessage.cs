using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class ChatMessage : ServerMessage
    {
        public ChatMessage(string messageData)
            : base(messageData)
        {
            Text = Items["txt"];
            RoomId = int.Parse(Items["rid"]);
            UserId = int.Parse(Items["uid"]);
            UserName = Items["nn"];
            UserLevel = int.Parse(Items["level"]);
            BadgeName = Items["bnn"];
            BadgeLevel = int.Parse(Items["bl"]);
            BadgeRoomId = int.Parse(Items["brid"]);
        }

        public string Text { get; private set; }
        public int RoomId { get; private set; }
        public int UserId { get; private set; }
        public string UserName { get; private set; }
        public int UserLevel { get; private set; }
        public string BadgeName { get; private set; }
        public int BadgeLevel { get; private set; }
        public int BadgeRoomId { get; private set; }

        public override string ToString()
        {
            return Text;
        }
    }
}

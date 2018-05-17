using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messages;

namespace Douyu.Messsages
{
    public class ChatMessage : ServerMessage
    {
        public ChatMessage(string messageText)
            : base(messageText)
        {
            if (MessageItems["type"] != "chatmsg")
                throw new MessageException("{0}不是弹幕消息!", messageText);

            Text = MessageItems["txt"];
            RoomId = int.Parse(MessageItems["rid"]);
            UserId = int.Parse(MessageItems["uid"]);
            UserName = MessageItems["nn"];
            UserLevel = int.Parse(MessageItems["level"]);
            BadgeName = MessageItems["bnn"];
            BadgeLevel = int.Parse(MessageItems["bl"]);
            BadgeRoomId = int.Parse(MessageItems["brid"]);
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

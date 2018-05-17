using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messages;

namespace Douyu.Messsages
{
    public class ChouqinMessage : ServerMessage
    {
        public ChouqinMessage(string messageText)
            : base(messageText)
        {
            if (MessageItems["type"] != "bc_buy_deserve")
                throw new MessageException("{0}不是酬勤消息!", messageText);

            RoomId = int.Parse(MessageItems["rid"]);
            Level = byte.Parse(MessageItems["lev"]);
            Count = short.Parse(MessageItems["cnt"]);
            Hits = short.Parse(MessageItems["hits"]);
            UserId = int.Parse(MessageItems["sid"]);
            UserLevel = byte.Parse(MessageItems["level"]);
            BadgeName = (string)MessageItems["bnn"];
            BadgeLevel = byte.Parse(MessageItems["bl"]);
            BadgeRoom = int.Parse(MessageItems["brid"]);
        }

        public int RoomId { get; private set; }
        public byte Level { get; private set; }
        public short Count { get; private set; }
        public short Hits { get; private set; }
        public int UserId { get; private set; }
        public byte UserLevel { get; private set; }
        public string BadgeName { get; private set; }
        public byte BadgeLevel { get; private set; }
        public int BadgeRoom { get; private set; }
    }
}

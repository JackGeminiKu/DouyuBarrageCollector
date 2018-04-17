using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class ChouqinMessage : ServerMessage
    {
        public ChouqinMessage(string messageData)
            : base(messageData)
        {
            RoomId = int.Parse(Items["rid"]);
            Level = byte.Parse(Items["lev"]);
            Count = short.Parse(Items["cnt"]);
            Hits = short.Parse(Items["hits"]);
            UserId = int.Parse(Items["sid"]);
            UserLevel = byte.Parse(Items["level"]);
            BadgeName = (string)Items["bnn"];
            BadgeLevel = byte.Parse(Items["bl"]);
            BadgeRoom = int.Parse(Items["brid"]);
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

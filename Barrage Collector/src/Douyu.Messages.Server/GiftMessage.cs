using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Client;

namespace Douyu.Messsages
{
    public class GiftMessage : ServerMessage
    {
        public GiftMessage(string messageData)
            : base(messageData)
        {
            RoomId = int.Parse(Items["rid"]);
            UserId = int.Parse(Items["uid"]);
            UserName = Items["nn"];
            UserLevel = int.Parse(Items["level"]);
            Weight = int.Parse(Items["dw"]);
            GiftId = int.Parse(Items["gfid"]);
            Hits = Items.ContainsKey("hits") ? int.Parse(Items["hits"]) : 0;
            BadgeName = Items["bnn"];
            BadgeLevel = int.Parse(Items["bl"]);
            BadgeRoomId = int.Parse(Items["brid"]);
        }

        public int RoomId { get; private set; }
        public int UserId { get; private set; }
        public string UserName { get; private set; }
        public int UserLevel { get; private set; }
        public int Weight { get; private set; }
        public int GiftId { get; private set; }

        public string GiftName
        {
            get
            {
                Dictionary<string, object> giftInfo = DbService.QueryGiftInfo(GiftId);
                return giftInfo == null ? GiftId.ToString() : (string)giftInfo["name"];
            }
        }

        public int Hits { get; private set; }
        public string BadgeName { get; private set; }
        public int BadgeLevel { get; private set; }
        public int BadgeRoomId { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", UserName, GiftName);
        }
    }
}

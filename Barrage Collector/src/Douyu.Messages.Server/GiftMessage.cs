using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Client;
using Douyu.Messages;
using System.Data.SqlClient;
using System.Data;
using Jack4net.Log;
using Dapper;

namespace Douyu.Messsages
{
    public class GiftMessage : ServerMessage
    {
        public GiftMessage(string messageText)
            : base(messageText)
        {
            if (MessageItems["type"] != "dgb")
                throw new MessageException("{0}不是礼物消息!", messageText);

            RoomId = int.Parse(MessageItems["rid"]);
            UserId = int.Parse(MessageItems["uid"]);
            UserName = MessageItems["nn"];
            UserLevel = int.Parse(MessageItems["level"]);
            Weight = int.Parse(MessageItems["dw"]);
            GiftId = int.Parse(MessageItems["gfid"]);
            Hits = MessageItems.ContainsKey("hits") ? int.Parse(MessageItems["hits"]) : 0;
            BadgeName = MessageItems["bnn"];
            BadgeLevel = int.Parse(MessageItems["bl"]);
            BadgeRoomId = int.Parse(MessageItems["brid"]);
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
                DouyuGift giftInfo = DouyuGift.Get(GiftId);
                return giftInfo == null ? GiftId.ToString() : giftInfo.Name;
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

        static IDbConnection _connection;

        static GiftMessage()
        {
            _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            _connection.Open();
        }

        public static void Save(GiftMessage giftMessage)
        {
            var count = _connection.Execute(
                "insert into GiftMessage(Time, RoomId, UserId, UserName, UserLevel, Weight, GiftId, Hits, BadgeName, BadgeLevel, BadgeRoom) " +
                "values(@Time, @RoomId, @UserId, @UserName, @UserLevel, @Weight, @GiftId, @Hits, @BadgeName, @BadgeLevel, @BadgeRoom)",
                new {
                    Time = DateTime.Now,
                    RoomId = giftMessage.RoomId,
                    UserId = giftMessage.UserId,
                    UserName = giftMessage.UserName,
                    UserLevel = giftMessage.UserLevel,
                    Weight = giftMessage.Weight,
                    GiftId = giftMessage.GiftId,
                    Hits = giftMessage.Hits,
                    BadgeName = giftMessage.BadgeName,
                    BadgeLevel = giftMessage.BadgeLevel,
                    BadgeRoom = giftMessage.BadgeRoomId
                });
            if (count != 1)
                LogService.InfoFormat("保存礼物失败: 返回值不为1!");
        }
    }
}

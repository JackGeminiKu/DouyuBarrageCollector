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
            Gift = Gift.GetGift(MessageItems["gfid"]);
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
        public Gift Gift { get; private set; }
        public int Hits { get; private set; }
        public string BadgeName { get; private set; }
        public int BadgeLevel { get; private set; }
        public int BadgeRoomId { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", UserName, Gift == null ? "未知礼物" : Gift.Name);
        }

        static IDbConnection _connection;

        public static void Save(GiftMessage message)
        {
            if (_connection == null)
                _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            var count = _connection.Execute(
                "insert into " +
                "GiftMessage(Time, RoomId, UserId, UserName, UserLevel, Weight, GiftId, GiftName, GiftPrice, GiftExperience, GiftDevote, Hits, BadgeName, BadgeLevel, BadgeRoom) " +
                "values(@Time, @RoomId, @UserId, @UserName, @UserLevel, @Weight, @GiftId, @GiftName, @GiftPrice, @GiftExperience, @GiftDevote, @Hits, @BadgeName, @BadgeLevel, @BadgeRoom)",
                new {
                    Time = message.Time,
                    RoomId = message.RoomId,
                    UserId = message.UserId,
                    UserName = message.UserName,
                    UserLevel = message.UserLevel,
                    Weight = message.Weight,
                    GiftId = message.Gift.Id,
                    GiftName = message.Gift.Name,
                    GiftPrice = message.Gift.Price,
                    GiftExperience = message.Gift.Experience,
                    GiftDevote = message.Gift.Devote,
                    Hits = message.Hits,
                    BadgeName = message.BadgeName,
                    BadgeLevel = message.BadgeLevel,
                    BadgeRoom = message.BadgeRoomId
                });
        }
    }
}

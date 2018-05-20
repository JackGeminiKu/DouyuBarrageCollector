using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messages;
using Dapper;
using Jack4net.Log;
using System.Data;
using System.Data.SqlClient;

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

        public override string ToString()
        {
            return "酬勤" + Level;
        }

        static IDbConnection _connection;

        static ChouqinMessage()
        {
            _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            _connection.Open();
        }

        public static void Save(ChouqinMessage message)
        {
            var count = _connection.Execute(
                "insert into chouqin_message([time], [room_id], [level], [count], [hits], [user_id], [user_level], [badge_name], [badge_level], [badge_room]) " +
                "values(@Time, @RoomId, @Level, @Count, @Hits, @UserId, @UserLevel, @BadgeName, @BadgeLevel, @BadgeRoom)",
                new {
                    Time = DateTime.Now,
                    RoomId = message.RoomId,
                    Level = message.Level,
                    Count = message.Count,
                    Hits = message.Hits,
                    UserId = message.UserId,
                    UserLevel = message.UserLevel,
                    BadgeName = message.BadgeName,
                    BadgeLevel = message.BadgeLevel,
                    BadgeRoom = message.BadgeRoom
                });
            if (count != 1)
                LogService.Info("保存酬勤失败: 返回值不为1!");
        }
    }
}
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
            Level = int.Parse(MessageItems["lev"]);
            Count = int.Parse(MessageItems["cnt"]);
            Hits = int.Parse(MessageItems["hits"]);
            UserId = int.Parse(MessageItems["sid"]);
            UserLevel = int.Parse(MessageItems["level"]);
            BadgeName = MessageItems["bnn"];
            BadgeLevel = int.Parse(MessageItems["bl"]);
            BadgeRoom = int.Parse(MessageItems["brid"]);
        }

        public int RoomId { get; private set; }
        public int Level { get; private set; }
        public int Count { get; private set; }
        public int Hits { get; private set; }
        public int UserId { get; private set; }
        public int UserLevel { get; private set; }
        public string BadgeName { get; private set; }
        public int BadgeLevel { get; private set; }
        public int BadgeRoom { get; private set; }

        public override string ToString()
        {
            return "酬勤" + Level;
        }

        static IDbConnection _connection;

        public static void Save(ChouqinMessage message)
        {
            if (_connection == null)
                _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            var count = _connection.Execute(
                "insert into " +
                "ChouqinMessage([Time], [RoomId], [Level], [Count], [Hits], [UserId], [UserLevel], [BadgeName], [BadgeLevel], [BadgeRoom]) " +
                "values(@Time, @RoomId, @Level, @Count, @Hits, @UserId, @UserLevel, @BadgeName, @BadgeLevel, @BadgeRoom)",
                new {
                    Time = message.Time,
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
        }
    }
}
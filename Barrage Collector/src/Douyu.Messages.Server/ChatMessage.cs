using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messages;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using My.Log;
using System.Diagnostics;

namespace Douyu.Messsages
{
    public class ChatMessage : ServerMessage
    {
        public ChatMessage(string messageText)
            : base(messageText)
        {
            if (MessageItems["type"] != "chatmsg")
                throw new MessageException("{0}不是弹幕消息!", messageText);
            SendingTime = MessageItems.ContainsKey("cst") ? GetTime(long.Parse(MessageItems["cst"])) : DateTime.Now;
            Text = MessageItems["txt"];
            RoomId = int.Parse(MessageItems["rid"]);
            UserId = int.Parse(MessageItems["uid"]);
            UserName = MessageItems["nn"];
            UserLevel = int.Parse(MessageItems["level"]);
            BadgeName = MessageItems["bnn"];
            BadgeLevel = int.Parse(MessageItems["bl"]);
            BadgeRoomId = int.Parse(MessageItems["brid"]);
        }

        DateTime GetTime(long timeStamp)
        {
            if (timeStamp.ToString().Length == 10)
                timeStamp *= 1000;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return startTime.AddMilliseconds(timeStamp);
        }

        public DateTime SendingTime { get; private set; }
        public string Text { get; set; }
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

        static IDbConnection _connection;

        public static void Save(ChatMessage message)
        {
            if (_connection == null)
                _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            var count = _connection.Execute(
                "insert into " +
                "ChatMessage(Time, SendingTime, Text, RoomId, UserId, UserName, UserLevel, BadgeName, BadgeLevel, BadgeRoom) " +
                "values(@Time, @SendingTime, @Text, @RoomId, @UserId, @UserName, @UserLevel, @BadgeName, @BadgeLevel, @BadgeRoom)",
                new {
                    Time = message.Time,
                    SendingTime = message.SendingTime,
                    Text = message.Text.Replace("'", "''"),
                    RoomId = message.RoomId,
                    UserId = message.UserId,
                    UserName = message.UserName,
                    UserLevel = message.UserLevel,
                    BadgeName = message.BadgeName,
                    BadgeLevel = message.BadgeLevel,
                    BadgeRoom = message.BadgeRoomId
                });
        }
    }
}

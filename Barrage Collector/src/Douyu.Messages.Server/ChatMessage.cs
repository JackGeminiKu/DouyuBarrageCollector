using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messages;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Jack4net.Log;

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

        static IDbConnection _connection;

        public static void Save(ChatMessage chatMessage)
        {
            if (_connection == null)
                _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            var count = _connection.Execute(
                @"insert into " +
                "ChatMessage(Time, Text, RoomId, UserId, UserName, UserLevel, BadgeName, BadgeLevel, BadgeRoom) " +
                "values(@Time, @Text, @RoomId, @UserId, @UserName, @UserLevel, @BadgeName, @BadgeLevel, @BadgeRoom)",
                new {
                    Time = DateTime.Now,
                    Text = chatMessage.Text.Replace(@"'", @"''"),
                    RoomId = chatMessage.RoomId,
                    UserId = chatMessage.UserId,
                    UserName = chatMessage.UserName,
                    UserLevel = chatMessage.UserLevel,
                    BadgeName = chatMessage.BadgeName,
                    BadgeLevel = chatMessage.BadgeLevel,
                    BadgeRoom = chatMessage.BadgeRoomId
                });
            if (count != 1)
                LogService.Info("保存弹幕失败: 返回值不为1!");
        }
    }
}

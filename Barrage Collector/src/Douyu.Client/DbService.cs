using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using Jack4net.Log;
using Douyu.Messsages;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Douyu.Client
{
    public static class DbService
    {
        static SqlConnection _conn;
        static Dictionary<int, Dictionary<string, object>> _gifts = new Dictionary<int, Dictionary<string, object>>();

        static DbService()
        {
            //var connectString = @"Data Source=10.0.0.2;Initial Catalog=Douyu2;User ID=sa;Password=Jack52664638";
            var connectString = @"Data Source=(local)\SQLServer2008;Initial Catalog=Douyu2;User ID=sa;Password=52664638";
            _conn = new SqlConnection(connectString);
            _conn.Open();
            UpdateGiftInfo();
        }

        static void UpdateGiftInfo()
        {
            IDataReader reader = null;
            try {
                reader = ExecuteReader("select id, name, price, experience from gift_category");
                while (reader.Read()) {
                    Dictionary<string, object> giftInfo = new Dictionary<string, object>();
                    giftInfo.Add("id", reader["id"]);
                    giftInfo.Add("name", reader["name"]);
                    giftInfo.Add("price", reader["price"]);
                    giftInfo.Add("experience", reader["experience"]);
                    _gifts.Add((int)reader["id"], giftInfo);
                }
            } finally {
                if (reader != null) reader.Close();
            }
        }

        public static void SaveChatMessage(ChatMessage chatMessage)
        {
            string command = string.Format("insert into chat_message" +
                "(time, text, room_id, user_id, user_name, user_level, badge_name, badge_level, badge_room) " +
                "values({0}, '{1}', {2}, {3}, '{4}', {5}, '{6}', {7}, {8})",
                "GETDATE()",
                chatMessage.Text.Replace(@"'", @"''"),
                chatMessage.RoomId,
                chatMessage.UserId, chatMessage.UserName, chatMessage.UserLevel,
                chatMessage.BadgeName, chatMessage.BadgeLevel, chatMessage.BadgeRoomId);
            if (ExecuteNonQuery(command) != 1)
                LogService.GetLogger("Error").InfoFormat("保存弹幕失败: 返回值不为1! ({0})", command);
        }

        public static void SaveGiftMessage(GiftMessage giftMessage)
        {
            string command = string.Format("insert into gift_message" +
                "(time, room_id, user_id, user_name, user_level, weight, gift_id, hits, badge_name, badge_level, badge_room) " +
                "values({0}, {1}, {2}, '{3}', {4}, {5}, {6}, {7}, '{8}', {9}, {10})",
                "GETDATE()",
                giftMessage.RoomId,
                giftMessage.UserId, giftMessage.UserName, giftMessage.UserLevel, giftMessage.Weight,
                giftMessage.GiftId, giftMessage.Hits,
                giftMessage.BadgeName, giftMessage.BadgeLevel, giftMessage.BadgeRoomId);
            if (ExecuteNonQuery(command) != 1)
                LogService.GetLogger("Error").InfoFormat("保存礼物失败: 返回值不为1! ({0})", command);
        }

        public static void SaveChouqinMessage(ChouqinMessage message)
        {
            string command = string.Format("insert into chouqin_message" +
                "([time], [room_id], [level], [count], [hits], [user_id], [user_level], [badge_name], [badge_level], [badge_room]) " +
                "values({0}, {1}, {2}, {3}, {4}, {5}, {6}, '{7}', {8}, {9})",
                "GETDATE()", message.RoomId,
                message.Level, message.Count, message.Hits,
                message.UserId, message.UserLevel,
                message.BadgeName, message.BadgeLevel, message.BadgeRoom);
            if (ExecuteNonQuery(command) != 1)
                LogService.GetLogger("Error").InfoFormat("保存酬勤失败: 返回值不为1! ({0})", command);
        }

        public static Dictionary<string, object> QueryGiftInfo(int giftId)
        {
            if (_gifts.ContainsKey(giftId)) {
                return _gifts[giftId];
            }

            IDataReader reader = null;
            try {
                reader = ExecuteReader(
                    "select id, name, price, experience from gift_category where id = {0}", giftId
                );
                if (reader.Read() == false) return null;

                Dictionary<string, object> giftInfo = new Dictionary<string, object>();
                giftInfo.Add("id", reader["id"]);
                giftInfo.Add("name", reader["name"]);
                giftInfo.Add("price", reader["price"]);
                giftInfo.Add("experience", reader["experience"]);
                return _gifts[giftId] = giftInfo;
            } finally {
                if (reader != null) reader.Close();
            }
        }

        public static bool IsCollecting(string RoomId)
        {
            object isCollecting = ExecuteScalar("select is_collecting from room_status where room_id = {0}", RoomId);
            return isCollecting != null && (bool)isCollecting;
        }

        public static void SetCollecting(string RoomId, bool isCollecting)
        {
            if (HasRow("select * from room_status where room_id = " + RoomId)) {
                string updateCommand = string.Format("update room_status set is_collecting = {0} where room_id = {1}",
                    isCollecting ? 1 : 0, RoomId);
                if (ExecuteNonQuery(updateCommand) != 1) {
                    MessageBox.Show("SetCollecting 失败: " + updateCommand, "Set Collecting");
                    return;
                }
            } else {
                string insertCommand = string.Format("insert into room_status(room_id, is_collecting) values({0}, {1})",
                    RoomId, isCollecting ? 1 : 0);
                if (ExecuteNonQuery(insertCommand) != 1) {
                    MessageBox.Show("SetCollecting 失败: " + insertCommand, "Set Collecting");
                    return;
                }
            }
        }

        #region "SQL functions"

        static SqlCommand CreateCommand(string sql)
        {
            if (_conn.State != ConnectionState.Open)
                _conn.Open();
            SqlCommand command = new SqlCommand(sql, _conn);
            command.CommandTimeout = 60;
            return command;
        }

        static int ExecuteNonQuery(string sql)
        {
            try {
                LogService.Debug("[SQL] " + sql);
                return CreateCommand(sql).ExecuteNonQuery();
            } catch (Exception) {
                LogService.Debug("[SQL-2] " + sql);
                return CreateCommand(sql).ExecuteNonQuery();
            }
        }

        static int ExecuteNonQuery(string sql, params object[] args)
        {
            return ExecuteNonQuery(string.Format(sql, args));
        }

        static object ExecuteScalar(string sql)
        {
            try {
                LogService.Debug("[SQL] " + sql);
                return CreateCommand(sql).ExecuteScalar();
            } catch (Exception) {
                LogService.Debug("[SQL-2] " + sql);
                return CreateCommand(sql).ExecuteScalar();
            }
        }

        static object ExecuteScalar(string sql, params object[] args)
        {
            return ExecuteScalar(string.Format(sql, args));
        }

        static bool HasRow(string sql)
        {
            try {
                LogService.Debug("[SQL] " + sql);
                return CreateCommand(sql).ExecuteScalar() != null;
            } catch (Exception) {
                LogService.Debug("[SQL-2] " + sql);
                return CreateCommand(sql).ExecuteScalar() != null;
            }
        }

        static IDataReader ExecuteReader(string sql)
        {
            try {
                LogService.Debug("[SQL] " + sql);
                return CreateCommand(sql).ExecuteReader();
            } catch (Exception) {
                LogService.Debug("[SQL-2] " + sql);
                return CreateCommand(sql).ExecuteReader();
            }
        }

        static IDataReader ExecuteReader(string sql, params object[] args)
        {
            return ExecuteReader(string.Format(sql, args));
        }

        #endregion
    }
}

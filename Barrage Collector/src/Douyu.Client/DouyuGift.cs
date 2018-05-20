using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Douyu.Client
{
    public class DouyuGift
    {
        public int Id { get; set; }
        public string name { get; set; }
        public float price { get; set; }
        public float experience { get; set; }
        public string description { get; set; }
        public string introduction { get; set; }
        public string mimg { get; set; }
        public string himg { get; set; }
        public DateTime update_time { get; set; }

        static IDbConnection _connection;
        static Dictionary<int, DouyuGift> _gifts = new Dictionary<int, DouyuGift>();

        static DouyuGift()
        {
            _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            _connection.Open();
            UpdateGiftInfo();
        }

        static void UpdateGiftInfo()
        {
            var giftCategories = _connection.Query<DouyuGift>(@"select * from gift_category");
            foreach (var giftInfo in giftCategories) {
                _gifts.Add(giftInfo.Id, giftInfo);
            }
        }

        public static DouyuGift Get(int giftId)
        {
            if (_gifts.ContainsKey(giftId)) {
                return _gifts[giftId];
            }

            var giftInfoList = _connection.Query<DouyuGift>(
                @"select * from gift_category where id = @GiftId",
                new { GiftId = giftId });
            if (giftInfoList.Count() == 0)
                return null;

            return _gifts[giftId] = giftInfoList.ElementAt(0);
        }
    }
}

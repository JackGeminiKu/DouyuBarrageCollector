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
        public string Name { get; set; }
        public float Price { get; set; }
        public float Experience { get; set; }
        public string Description { get; set; }
        public string Introduction { get; set; }
        public string Mimg { get; set; }
        public string Himg { get; set; }
        public DateTime UpdateTime { get; set; }

        static IDbConnection _connection;
        static Dictionary<int, DouyuGift> _gifts = new Dictionary<int, DouyuGift>();

        static DouyuGift()
        {
            _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            UpdateGiftInfo();
        }

        static void UpdateGiftInfo()
        {
            var giftCategories = _connection.Query<DouyuGift>(@"select * from GiftCategory");
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
                @"select * from GiftCategory where Id = @GiftId",
                new { GiftId = giftId });
            if (giftInfoList.Count() == 0)
                return null;

            return _gifts[giftId] = giftInfoList.ElementAt(0);
        }
    }
}

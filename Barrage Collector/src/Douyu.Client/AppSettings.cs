using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Douyu.Client
{
    public static class AppSettings
    {
        static IDbConnection _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);

        public static int RoomId { get { return 122402; } }

        public static string GiftUrls
        {
            get
            {
                ConfigurationManager.RefreshSection("appSettings");
                return ConfigurationManager.AppSettings["GiftUrls"];
            }
        }
    }
}

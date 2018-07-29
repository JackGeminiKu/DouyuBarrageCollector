using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using System.IO;

namespace Douyu.Client
{
    public class RoomPage
    {
        public static IPEndPoint[] GetServers(int roomId)
        {
            const string SERVER_CONFIG = "server_config";

            var roomPage = GetRoomPage(roomId);
            var index = roomPage.IndexOf(SERVER_CONFIG);
            index = roomPage.IndexOf("\"", index + 1);
            index = roomPage.IndexOf("\"", index + 1);
            var firstIndex = index + 1;
            index = roomPage.IndexOf("\"", index + 1);
            var lastIndex = index - 1;
            var serverConfig = HttpUtility.UrlDecode(
                roomPage.Substring(firstIndex, lastIndex - firstIndex + 1), Encoding.ASCII);
            var servers = JsonConvert.DeserializeObject<dynamic>(serverConfig);
            var douyuServers = new List<IPEndPoint>();
            foreach (var server in servers) {
                douyuServers.Add(new IPEndPoint(IPAddress.Parse(server.ip.Value), int.Parse(server.port.Value)));
            }

            if (douyuServers.Count == 0)
                throw new DouyuException("没有找到斗鱼服务器");
            return douyuServers.ToArray();
        }

        static string GetRoomPage(int roomId)
        {
            Stream stream = null;
            StreamReader reader = null;
            try {
                var uri = string.Format("http://www.douyu.com/{0}", roomId);
                var request = HttpWebRequest.Create(uri) as HttpWebRequest;
                request.KeepAlive = true;
                request.ProtocolVersion = HttpVersion.Version11;
                request.Method = "GET";
                request.Accept = "*/* ";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.5 (KHTML, like Gecko) " +
                    "Chrome/19.0.1084.56 Safari/536.5";
                request.Referer = uri;
                stream = request.GetResponse().GetResponseStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                var roomPage = reader.ReadToEnd();
                return roomPage;
            } finally {
                if (stream != null)
                    stream.Close();
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
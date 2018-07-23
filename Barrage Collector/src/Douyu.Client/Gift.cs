using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Douyu.Client
{
    public class Gift
    {
        public Gift(string id, string name, int price, int experience, double devote)
        {
            Id = id;
            Name = name;
            Price = price;
            Experience = experience;
            Devote = devote;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public int Price { get; private set; } // 单位是分
        public int Experience { get; private set; }
        public double Devote { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Id, Name);
        }

        static Dictionary<string, Gift> _gifts;

        public static Gift GetGift(string giftId)
        {
            if (_gifts == null || !_gifts.ContainsKey(giftId)) {
                _gifts = GetGifts();
            }

            return _gifts.ContainsKey(giftId) ? _gifts[giftId] : new Gift(giftId, "未知礼物", 0, 0, 0);
        }

        static Dictionary<string, Gift> GetGifts()
        {
            var gifts = new Dictionary<string, Gift>();
            gifts = GetPropGifts();

            var gifts2 = new Dictionary<string, Gift>();
            gifts2 = Get635Gifts();

            var gifts3 = new Dictionary<string, Gift>();
            gifts3 = Get54Gifts();

            foreach (var item in gifts2.Keys) {
                if (!gifts.ContainsKey(item)) {
                    gifts.Add(item, gifts2[item]);
                }
            }

            foreach (var item in gifts3.Keys) {
                if (!gifts.ContainsKey(item)) {
                    gifts.Add(item, gifts3[item]);
                }
            }

            foreach (var item in gifts.Values) {
                Console.WriteLine("{0}, {1}, {2}, {3}, {4}", item.Id, item.Name, item.Price, item.Experience, item.Devote);
            }

            return gifts;
        }

        static Dictionary<string, Gift> GetPropGifts()
        {
            var url = "https://webconf.douyucdn.cn/resource/common/prop_gift_list/prop_gift_config.json";
            var json = GetGiftJson(url);
            var propGiftConfig = JsonConvert.DeserializeObject<dynamic>(json);
            if (propGiftConfig.error != 0)
                return null;

            var gifts = new Dictionary<string, Gift>();
            foreach (var item in propGiftConfig.data) {
                var gift = new Gift(
                    item.Name,
                    item.Value["name"].Value.ToString(),
                    (int)item.Value["pc"],
                    (int)item.Value["exp"],
                    (double)item.Value["devote"]
                );
                gifts.Add(item.Name, gift);
            }

            return gifts;
        }

        static Dictionary<string, Gift> Get635Gifts()
        {
            var url = "https://webconf.douyucdn.cn/resource/common/gift/gift_template/625.json";
            var json = GetGiftJson(url);
            var propGiftConfig = JsonConvert.DeserializeObject<dynamic>(json);
            if (propGiftConfig.error != 0)
                return null;

            var gifts = new Dictionary<string, Gift>();
            foreach (var item in propGiftConfig.data) {
                var gift = new Gift(
                    item["id"].ToString(),
                    item["name"].ToString(),
                    (int)item["pc"],
                    (int)item["exp"],
                    (double)item["devote"]
                );
                gifts.Add(item["id"].ToString(), gift);
            }

            return gifts;
        }

        static Dictionary<string, Gift> Get54Gifts()
        {
            var url = "https://webconf.douyucdn.cn/resource/common/gift/gift_template/54.json";
            var json = GetGiftJson(url);
            var propGiftConfig = JsonConvert.DeserializeObject<dynamic>(json);
            if (propGiftConfig.error != 0)
                return null;

            var gifts = new Dictionary<string, Gift>();
            foreach (var item in propGiftConfig.data) {
                var gift = new Gift(
                    item["id"].ToString(),
                    item["name"].ToString(),
                    (int)item["pc"],
                    (int)item["exp"],
                    (double)item["devote"]
                );
                gifts.Add(item["id"].ToString(), gift);
            }

            return gifts;
        }

        static string GetGiftJson(string url)
        {
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.KeepAlive = true;
            request.ProtocolVersion = HttpVersion.Version11;
            request.Method = "GET";
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.56 Safari/536.";
            request.Referer = url;
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var page = reader.ReadToEnd();
            var firstBrace = page.IndexOf('{');
            var lastBrace = page.LastIndexOf('}');
            var json = page.Substring(firstBrace, lastBrace - firstBrace + 1);
            reader.Close();
            stream.Close();
            response.Close();
            return json;
        }
    }
}

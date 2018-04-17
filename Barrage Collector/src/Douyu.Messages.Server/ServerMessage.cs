using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class ServerMessage : Message
    {
        public ServerMessage(string messageData)
        {
            MessageData = messageData;
            Items = ParseServerMessage(messageData);
        }

        public Dictionary<string, string> Items { get; private set; }

        Dictionary<string, string> ParseServerMessage(string messageData)
            {
            // 去除字符串末尾的'/0字符'
            messageData = messageData.Trim('\0');

            // 分析协议字段中的key和value值
            var items = new Dictionary<string, string>();
            var values = messageData.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in values) {
                int separatorStart = item.IndexOf("@=", StringComparison.Ordinal);
                string key = item.Substring(0, separatorStart);
                var value = item.Substring(separatorStart + 2);

                // 子序列化暂时不处理!!!!
                ////// 如果value值中包含子序列化值，则进行递归分析
                ////if (((string)value).Contains("@A")) {
                ////    value = ((string)value).Replace("@S", "/").Replace("@A", "@");
                ////    value = this.ParseRespond((string)value);
                ////}

                items.Add(key, value);
            }

            return items;
        }

        protected override int MessageType
        {
            get { return 690; }
        }

        public override string ToString()
        {
            return MessageData;
        }
    }
}

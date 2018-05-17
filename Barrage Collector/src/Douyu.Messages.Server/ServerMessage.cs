using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class ServerMessage : Message
    {
        public ServerMessage(string messageText)
        {
            MessageText = messageText;
            MessageItems = ParseServerMessage(messageText);
        }

        Dictionary<string, string> ParseServerMessage(string messageText)
        {
            // 去除字符串末尾的'/0字符'
            messageText = messageText.Trim('\0');

            // 分析协议字段中的key和value值
            var messageItems = new Dictionary<string, string>();
            foreach (var item in messageText.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)) {
                int separatorStart = item.IndexOf("@=", StringComparison.Ordinal);
                string key = item.Substring(0, separatorStart);
                var value = item.Substring(separatorStart + 2);

                // 子序列化暂时不处理!!!!
                ////// 如果value值中包含子序列化值，则进行递归分析
                ////if (((string)value).Contains("@A")) {
                ////    value = ((string)value).Replace("@S", "/").Replace("@A", "@");
                ////    value = this.ParseRespond((string)value);
                ////}

                messageItems.Add(key, value);
            }

            return messageItems;
        }

        protected override int MessageType
        {
            get { return 690; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jack4net.Log;

namespace Douyu.Messsages
{
    public abstract class Message
    {
        public Message()
        {
            Time = DateTime.Now;
        }

        public DateTime Time { get; protected set; }

        protected abstract int MessageType { get; }
        public string MessageData { get; protected set; }
        public byte[] MessgeBytes { get { return CreateMessageBytes(MessageData); } }

        byte[] CreateMessageBytes(string messageData)
        {
            messageData += "\0";
            var result = new List<byte>();

            try {
                var lenBytes = (messageData.Length + 8).ToLittleEndian();
                result.AddRange(lenBytes);
                result.AddRange(lenBytes);
                result.AddRange(MessageType.ToLittleEndian().Take(2));
                result.Add(0);
                result.Add(0);
                result.AddRange(Encoding.UTF8.GetBytes(messageData));
            } catch (Exception e) {
                LogService.GetLogger("Error").Error(e.Message, e);
                throw;
            }

            return result.ToArray<byte>();
        }

        protected void AddItem(string key, string value)
        {
            MessageData += string.Format("{0}@={1}/", ConvertKeyWord(key), ConvertKeyWord(value));
        }

        string ConvertKeyWord(string value)
        {
            return value.Replace("/", "@S").Replace("@", "@A");
        }

        public override string ToString()
        {
            return MessageData;
        }
    }
}

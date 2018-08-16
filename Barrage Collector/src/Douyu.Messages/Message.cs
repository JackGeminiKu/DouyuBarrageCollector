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

        public DateTime Time { get; private set; }

        protected abstract int MessageType { get; }

        public string MessageText { get; protected set; }

        public byte[] MessgeBytes { get { return ConvertToMessageBytes(MessageText); } }

        public Dictionary<string, string> MessageItems { get; protected set; }

        byte[] ConvertToMessageBytes(string messageText)
        {
            if (!messageText.EndsWith("\0"))
                messageText += "\0";

            var result = new List<byte>();
            try {
                var lenBytes = (messageText.Length + 8).ToLittleEndian();
                result.AddRange(lenBytes);
                result.AddRange(lenBytes);
                result.AddRange(MessageType.ToLittleEndian().Take(2));
                result.Add(0);
                result.Add(0);
                result.AddRange(Encoding.UTF8.GetBytes(messageText));
            } catch (Exception e) {
                LogService.Error(e.Message, e);
                throw;
            }

            return result.ToArray<byte>();
        }

        protected void AddMessageItem(string key, string value)
        {
            MessageText += string.Format("{0}@={1}/", ConvertKeyWord(key), ConvertKeyWord(value));
        }

        string ConvertKeyWord(string value)
        {
            return value.Replace("/", "@S").Replace("@", "@A");
        }

        public override string ToString()
        {
            return MessageText;
        }
    }
}

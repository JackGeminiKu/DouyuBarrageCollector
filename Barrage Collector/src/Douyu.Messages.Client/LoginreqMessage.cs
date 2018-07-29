using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Douyu.Messsages
{
    public class LoginreqMessage : ClientMessage
    {
        public LoginreqMessage()
        {
            AddMessageItem("type", "loginreq");
            AddMessageItem("roomid", "742805");
        }

        public LoginreqMessage(int roomId)
        {
            AddMessageItem("type", "loginreq");
            AddMessageItem("username", "");
            AddMessageItem("ct", "0");
            AddMessageItem("password", "");
            AddMessageItem("roomid", roomId.ToString());
            var guid = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            //guid = "D4F18B38EBD24535BD7E38D8642543A1";
            AddMessageItem("devid", guid);
            var timestamp = ((long)((DateTime.UtcNow - new DateTime(1970, 1, 1,0,0,0,0)).TotalSeconds)).ToString();
            //timestamp = "1532612830";
            AddMessageItem("rt", timestamp);
            AddMessageItem("vk", MD5Encrypt(timestamp + "7oE9nPEG9xXV69phU31FYCLUagKeYtsF" + guid, 32));
            AddMessageItem("ver", "20150929");
        }   

        string MD5Encrypt(string password, int bit)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes;
            hashedDataBytes = md5Hasher.ComputeHash(Encoding.GetEncoding("gb2312").GetBytes(password));
            StringBuilder tmp = new StringBuilder();
            foreach (byte i in hashedDataBytes) {
                tmp.Append(i.ToString("x2"));
            }
            if (bit == 16)
                return tmp.ToString().Substring(8, 16);
            else
                if (bit == 32) return tmp.ToString();//默认情况
                else return string.Empty;
        }
    }
}

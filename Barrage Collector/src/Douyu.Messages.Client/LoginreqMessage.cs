using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class LoginreqMessage : ClientMessage
    {
        public LoginreqMessage(string RoomId)
        {
            AddItem("type", "loginreq");  // 添加登录协议type类型            
            AddItem("roomid", RoomId);  // 添加登录房间ID
        }
    }
}

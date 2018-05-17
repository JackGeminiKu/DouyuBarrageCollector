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
            AddMessageItem("type", "loginreq");      
            AddMessageItem("roomid", RoomId); 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class LogoutMessage : ClientMessage
    {
        public LogoutMessage()
        {
            AddItem("type", "logout");  // 添加登录协议type类型                        
        }
    }
}

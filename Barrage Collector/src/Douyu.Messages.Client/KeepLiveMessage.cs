using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messsages;

namespace Douyu.Messsages
{
    public class KeepLiveMessage : ClientMessage
    {
        public KeepLiveMessage()
        {
            AddItem("type", "mrkl");  // 添加心跳协议type类型                        
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class JoinGroupMessage : ClientMessage
    {
        public JoinGroupMessage(string roomId)
        {
            AddMessageItem("type", "joingroup");    
            AddMessageItem("rid", roomId);  // 添加房间id信息
            AddMessageItem("gid", "-9999");  // 海量弹幕
        }
    }
}

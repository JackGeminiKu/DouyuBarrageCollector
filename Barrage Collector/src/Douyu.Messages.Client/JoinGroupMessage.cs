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
            AddItem("type", "joingroup");  // 添加加入弹幕池协议type类型            
            AddItem("rid", roomId);  // 添加房间id信息
            AddItem("gid", "-9999");  // 海量弹幕
        }
    }
}

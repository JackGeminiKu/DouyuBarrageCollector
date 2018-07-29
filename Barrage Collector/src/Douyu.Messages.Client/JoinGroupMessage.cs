using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class JoinGroupMessage : ClientMessage
    {
        public JoinGroupMessage(int roomId, int messageGroup)
        {
            AddMessageItem("type", "joingroup");
            AddMessageItem("rid", roomId.ToString());  // 添加房间id信息
            //AddMessageItem("gid", "-9999");  // 海量弹幕
            AddMessageItem("gid", messageGroup.ToString());  // 海量弹幕
        }
    }
}

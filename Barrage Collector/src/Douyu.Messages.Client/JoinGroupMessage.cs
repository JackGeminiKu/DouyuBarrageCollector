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
            AddMessageItem("rid", roomId.ToString());  
            AddMessageItem("gid", messageGroup.ToString());  
        }
    }
}

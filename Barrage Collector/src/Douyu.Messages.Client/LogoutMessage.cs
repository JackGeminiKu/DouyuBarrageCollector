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
            AddMessageItem("type", "logout");              
        }
    }
}

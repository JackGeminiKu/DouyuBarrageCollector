using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messsages;

namespace Douyu.Messsages
{
    public class KeepliveMessage : ClientMessage
    {
        public KeepliveMessage()
        {
            AddMessageItem("type", "keeplive");
            AddMessageItem("tick", ((long)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds)).ToString());
        }
    }
}

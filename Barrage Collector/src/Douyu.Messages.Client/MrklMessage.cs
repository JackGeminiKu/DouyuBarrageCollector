﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Douyu.Messsages;

namespace Douyu.Messsages
{
    public class MrklMessage : ClientMessage
    {
        public MrklMessage()
        {
            AddMessageItem("type", "mrkl");                
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messsages
{
    public class ClientMessage : Message
    {
        protected override int MessageType
        {
            get { return 689; }
        }
    }
}

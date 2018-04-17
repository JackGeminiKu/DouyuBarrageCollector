using Douyu.Messsages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Events
{
    public class ClientMessageEventArgs : EventArgs
    {
        public ClientMessageEventArgs(ClientMessage clientMessage)
        {
            ClientMessage = clientMessage;
        }

        public ClientMessage ClientMessage { get; private set; }
    }
}

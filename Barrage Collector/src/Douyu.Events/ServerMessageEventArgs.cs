using Douyu.Messsages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Events
{
    public class ServerMessageEventArgs : EventArgs
    {
        public ServerMessageEventArgs(ServerMessage serverMessage)
        {
            ServerMessage = serverMessage;
        }

        public ServerMessage ServerMessage { get; private set; }
    }
}

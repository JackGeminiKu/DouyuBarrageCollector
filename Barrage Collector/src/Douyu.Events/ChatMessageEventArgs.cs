using Douyu.Messsages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Events
{
    public class ChatMessageEventArgs : EventArgs
    {
        public ChatMessageEventArgs(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }

        public ChatMessage ChatMessage { get; private set; }
    }
}

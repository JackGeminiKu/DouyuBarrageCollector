using Douyu.Messsages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Events
{
    public class MessageEventArgs<T> : EventArgs where T : Message
    {
        public MessageEventArgs(T message)
        {
            Message = message;
        }

        public T Message { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messages
{
    public class MessageException : Exception
    {
        public MessageException(string message)
            : base(message)
        { }

        public MessageException(string format, params object[] args)
            : this(string.Format(format, args))
        { }
    }
}

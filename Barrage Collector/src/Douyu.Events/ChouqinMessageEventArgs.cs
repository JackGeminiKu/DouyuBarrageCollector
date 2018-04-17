using Douyu.Messsages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Events
{
    public class ChouqinMessageEventArgs : EventArgs
    {
        public ChouqinMessageEventArgs(ChouqinMessage message)
        {
            ChouqinMessage = message;
        }

        public ChouqinMessage ChouqinMessage { get; private set; }
    }
}

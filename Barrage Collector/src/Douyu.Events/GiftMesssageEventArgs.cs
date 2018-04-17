using Douyu.Messsages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Events
{
    public class GiftMessageEventArgs : EventArgs
    {
        public GiftMessageEventArgs(GiftMessage giftMessage)
        {
            GiftMessage = giftMessage;
        }

        public GiftMessage GiftMessage { get; private set; }
    }
}

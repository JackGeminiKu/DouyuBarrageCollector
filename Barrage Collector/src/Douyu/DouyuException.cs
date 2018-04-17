using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu
{
    public class DouyuException : Exception
    {
        public DouyuException(String message)
            : base(message)
        { }
    }
}

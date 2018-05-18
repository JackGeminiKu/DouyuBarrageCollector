using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Client
{
    public class ScoreAddedEventArgs : EventArgs
    {
        public ScoreAddedEventArgs(string user, int score, string gift)
        {
            User = user;
            Score = score;
            Gift = gift;
        }

        public string User { get; private set; }
        public int Score { get; private set; }
        public string Gift { get; private set; }
    }
}

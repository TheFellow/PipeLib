using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeLib.Tests
{
    public static class Constants
    {
        public const int TIMEOUT_MS = 50;
        public const string TIMEOUT_CALLBACK = "Callbacks did not return in time.";
        public const string TIMEOUT_CONNECT = "Connections not established in time.";
        public const string TIMEOUT_DISCONNECT = "Disconnect not triggered in time.";
        public const string TIMEOUT_DATA = "Data not received in time.";
    }
}

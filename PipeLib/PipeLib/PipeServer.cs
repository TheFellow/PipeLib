using PipeLib.Core;
using PipeLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PipeLib
{
    public class PipeServer<T> : WriteMessage<T>, IConnectDisconnect
        where T : class
    {
        public Action OnConnect { get; set; }
        public Action OnDisconnect { get; set; }

        protected ServerPipe ServerPipe => (ServerPipe)BasicPipe;

        public PipeServer(string pipeName)
            : base()
        {
            BasicPipe = new ServerPipe(pipeName);
            ServerPipe.DataReceived += OnDataReceived;
            ServerPipe.PipeConnected += (sender, e) => OnConnect?.Invoke();
            ServerPipe.PipeClosed += (sender, e) => OnDisconnect?.Invoke();
        }

        public override string ToString() => $"S{ServerPipe.Id}<{typeof(T).Name}>";
    }
}

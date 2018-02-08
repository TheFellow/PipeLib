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
    public class PipeClient<T>
        : WriteMessage<T>, IConnectDisconnect, IConnectable
        where T : class
    {
        public Action OnConnect { get; set; }
        public Action OnDisconnect { get; set; }

        protected ClientPipe ClientPipe => (ClientPipe)BasicPipe;

        public PipeClient(string pipeName)
            : this(".", pipeName)
        {
        }

        public PipeClient(string pipeServer, string pipeName)
            : base()
        {
            BasicPipe = new ClientPipe(pipeServer, pipeName);
            ClientPipe.DataReceived += OnDataReceived;
            ClientPipe.PipeConnected += (sender, e) => OnConnect?.Invoke();
            ClientPipe.PipeClosed += (sender, e) => OnDisconnect?.Invoke();
        }

        public override string ToString() => $"C{ClientPipe.Id}<{typeof(T).Name}>";
        
        #region IConnectable via ClientPipe

        public void Connect() => ((IConnectable)ClientPipe).Connect();

        public void Connect(int timeout) => ((IConnectable)ClientPipe).Connect(timeout);

        public Task ConnectAsync() => ((IConnectable)ClientPipe).ConnectAsync();

        public Task ConnectAsync(int timeout) => ((IConnectable)ClientPipe).ConnectAsync(timeout);

        #endregion
    }
}

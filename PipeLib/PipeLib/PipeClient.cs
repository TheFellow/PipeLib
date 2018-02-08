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
    public class PipeClient : IConnectable
    {
        protected readonly ClientPipe _clientPipe;

        public PipeClient(string pipeServer, string pipeName)
        {
            _clientPipe = new ClientPipe(pipeServer, pipeName);
        }

        public PipeClient(string pipeName) : this(".", pipeName) { }

        #region IConnectable via _clientPipe

        public void Connect() => _clientPipe.Connect();

        public void Connect(int timeout) => _clientPipe.Connect(timeout);

        public Task ConnectAsync() => _clientPipe.ConnectAsync();

        public Task ConnectAsync(int timeout) => _clientPipe.ConnectAsync(timeout);

        #endregion
    }

    public class PipeClient<T> : PipeClient
        where T : class, new()
    {
        public PipeClient(string pipeName)
            : base(pipeName)
        {
            _clientPipe.DataReceived += OnDataReceived;
        }


        public PipeClient(string pipeServer, string pipeName)
            : base(pipeServer, pipeName)
        {
        }


        public void WriteObjectAsync(T obj)
        {
            var formatter = new BinaryFormatter();
            var ms = new MemoryStream();
            formatter.Serialize(ms, obj);
            _clientPipe.WriteBytesAsync(ms.ToArray());
        }

        public Action<T> Message;

        private void OnDataReceived(object sender, PipeEventArgs e)
        {
            var formatter = new BinaryFormatter();
            var ms = new MemoryStream(e.Data);
            Message?.Invoke((T)formatter.Deserialize(ms));
        }

        public override string ToString() => $"C{_clientPipe.Id}<{typeof(T).Name}>";
    }
}

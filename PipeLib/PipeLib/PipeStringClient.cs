using System;
using System.Text;

using PipeLib.Core;

namespace PipeLib
{
    public class PipeStringClient : PipeClientServerBase
    {
        public PipeStringClient(string pipeName)
            : base(pipeName)
        {
        }

        protected override BasicPipe CreatePipe() => new ClientPipe(".", PipeName, p => p.StartByteReader());

        protected override void DataReceived(object sender, PipeEventArgs e)
        {
            if(sender is BasicPipe pipe)
            {
                MessageReceived?.Invoke(pipe.Id, e.String);
            }
        }

        /// <summary>Method called when a message is received</summary>
        public Action<int, string> MessageReceived { get; set; }

        public void Send(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            foreach (int id in _connections.Keys)
                _connections[id].WriteBytesAsync(bytes);
        }

        public void Send(int id, string message)
        {
            if (!_connections.ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id));

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            _connections[id].WriteBytesAsync(bytes);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PipeLib.Core;
using PipeLib.Interfaces;

namespace PipeLib
{
    public class StringPipeClient : IConnectable, IWriteStringAsync, IDisposable
    {
        private readonly string _pipeName;
        private readonly ClientPipe clientPipe;

        public StringPipeClient(string pipeName)
        {
            _pipeName = pipeName;
            clientPipe = new ClientPipe(".", pipeName, p => p.StartStringReader());

            clientPipe.PipeConnected += OnConnect;
            clientPipe.PipeClosed += OnClose;
            clientPipe.DataReceived += OnDataReceived;
        }

        #region Public properties and Actions

        public string PipeName => _pipeName;

        public Action<int, string> MessageReceived { get; set; }
        public Action PipeConnected { get; set; }
        public Action PipeClosed { get; set; }

        #endregion

        #region IConnectable

        public void Connect() => clientPipe.Connect();

        public void Connect(int timeout) => clientPipe.Connect(timeout);

        public Task ConnectAsync() => clientPipe.ConnectAsync();

        public Task ConnectAsync(int timeout) => clientPipe.ConnectAsync(timeout);

        #endregion

        #region Connection handling

        private bool isConnected = false;

        public bool IsConnected => isConnected;


        private void OnConnect(object sender, EventArgs e)
        {
            isConnected = true;
            PipeConnected?.Invoke();
        }

        private void OnClose(object sender, EventArgs e)
        {
            isConnected = false;
            PipeClosed?.Invoke();
        }

        private void OnDataReceived(object sender, PipeEventArgs e)
        {
            if (sender is BasicPipe p)
            {
                MessageReceived?.Invoke(p.Id, e.String);
            }
        }

        #endregion

        public void Dispose() => clientPipe.Dispose();

        public Task WriteStringAsync(string str) => clientPipe.WriteStringAsync(str);
    }
}

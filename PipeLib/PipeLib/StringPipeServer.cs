using System;
using System.Threading.Tasks;

using PipeLib.Core;
using PipeLib.Interfaces;

namespace PipeLib
{
    public class StringPipeServer : IWriteStringAsync, IDisposable
    {
        private readonly string _pipeName;
        private readonly ServerPipe _serverPipe;

        public StringPipeServer(string pipeName)
        {
            _pipeName = pipeName;
            _serverPipe = new ServerPipe(pipeName, p => p.StartStringReader());

            _serverPipe.PipeConnected += OnConnect;
            _serverPipe.PipeClosed += OnClose;
            _serverPipe.DataReceived += OnDataReceived;
        }

        #region Public properties and Actions

        public string PipeName => _pipeName;

        public Action<int, string> MessageReceived { get; set; }
        public Action PipeConnected { get; set; }
        public Action PipeClosed { get; set; }

        #endregion

        #region Connection handling

        private bool isConnected;

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

        public void Dispose() => _serverPipe.Dispose();

        public Task WriteStringAsync(string str) => _serverPipe.WriteStringAsync(str);
    }
}

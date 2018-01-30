using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PipeLib.Core;

namespace PipeLib
{
    public abstract class PipeClientServerBase
    {
        private string _pipeName;
        protected BasicPipe _pipe;
        protected Dictionary<int, BasicPipe> _connections = new Dictionary<int, BasicPipe>();

        public PipeClientServerBase(string pipeName)
        {
            _pipeName = pipeName;
            InitPipe();
        }

        protected abstract BasicPipe CreatePipe();
        protected abstract void DataReceived(object sender, PipeEventArgs e);

        protected virtual void InitPipe()
        {
            _pipe = CreatePipe();
            _pipe.PipeConnected += PipeConnected;
            _pipe.PipeClosed += PipeClosed;
            _pipe.DataReceived += DataReceived;
        }

        private void PipeConnected(object sender, EventArgs e)
        {
            if (sender is BasicPipe pipe)
            {
                lock (_connections)
                {
                    _connections.Add(pipe.Id, pipe);
                    InitPipe(); // Wait for another connection
                }
            }
        }

        private void PipeClosed(object sender, EventArgs e)
        {
            if (sender is BasicPipe pipe)
            {
                lock (_connections)
                {
                    pipe.PipeClosed -= PipeClosed;
                    pipe.PipeConnected -= PipeConnected;
                    pipe.DataReceived -= DataReceived;
                    _connections.Remove(pipe.Id);
                }
            }
        }

        public int Count => _connections.Count;
        public string PipeName => _pipeName;
    }
}

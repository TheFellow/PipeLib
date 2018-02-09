/* The MIT License (MIT)
* 
* Copyright (c) 2015 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

// From: http://stackoverflow.com/questions/34478513/c-sharp-full-duplex-asynchronous-named-pipes-net
// See Eric Frazer's Q and self answer
//
// Based on Marc Clifton's CodeProject article: https://www.codeproject.com/Articles/1179195/Full-Duplex-Asynchronous-Read-Write-with-Named-Pip?msg=5480792#_comments
//

using PipeLib.Interfaces;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace PipeLib.Core
{
    public sealed class ClientPipe : BasicPipe, IConnectable
    {
        private NamedPipeClientStream ClientPipeStream => (NamedPipeClientStream)_pipeStream;

        public string ServerName { get; private set; }
        public string PipeName { get; private set; }

        /// <summary>Initialize a new instance of <see cref="ClientPipe"/></summary>
        /// <param name="serverName">The server name</param>
        /// <param name="pipeName">The pipe name</param>
        public ClientPipe(string serverName, string pipeName)
            : base()
        {
            ServerName = serverName;
            PipeName = pipeName;
            _pipeStream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        #region IConnectable via ClientPipeStream

        /// <summary>Wait for the client to connect. (Blocking)</summary>
        public void Connect()
        {
            ClientPipeStream.Connect();
            StartByteReader();
            RaisePipeConnected();
        }

        /// <summary>Wait for the client to connect given a timeout. (Blocking)</summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <exception cref="TimeoutException">Raised when the connection times out</exception>
        public void Connect(int timeout)
        {
            ClientPipeStream.Connect(timeout);
            ClientConnected();
        }

        /// <summary>Wait for the client to connect</summary>
        /// <returns>A task that represents the asynchronous connect operation.</returns>
        public async Task ConnectAsync()
        {
            await ClientPipeStream.ConnectAsync();
            ClientConnected();
        }

        /// <summary>Wait for the client to connect</summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>A task that represents the asynchronous connect operation.</returns>
        public async Task ConnectAsync(int timeout)
        {
            await ClientPipeStream.ConnectAsync(timeout);
            ClientConnected();
        }

        private void ClientConnected()
        {
            StartByteReader();
            RaisePipeConnected();
        }

        #endregion

    }
}

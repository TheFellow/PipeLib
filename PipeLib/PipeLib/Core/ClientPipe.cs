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

using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace PipeLib.Core
{
    public class ClientPipe : BasicPipe
    {
        protected NamedPipeClientStream ClientPipeStream => (NamedPipeClientStream)_pipeStream;

        public ClientPipe(string serverName, string pipeName, Action<BasicPipe> asyncReaderStart)
        {
            _asyncReaderStart = asyncReaderStart;
            _pipeStream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public void Connect() => ConnectAsync().GetAwaiter().GetResult();

        public void Connect(int timeout) => ConnectAsync(timeout).GetAwaiter().GetResult();

        public Task ConnectAsync() => ClientPipeStream.ConnectAsync()
            .ContinueWith(t =>
            {
                _asyncReaderStart(this);
                RaisePipeConnected();
            });

        public Task ConnectAsync(int timeout) => ClientPipeStream.ConnectAsync(timeout)
            .ContinueWith(t =>
            {
                _asyncReaderStart(this);
                RaisePipeConnected();
            });
    }
}

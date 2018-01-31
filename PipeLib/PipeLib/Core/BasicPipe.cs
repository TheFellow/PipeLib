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
using PipeLib.Utility;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeLib.Core
{
    /// <summary>
    /// Abstract base class for wrapping named pipes and their connection
    /// </summary>
    public abstract class BasicPipe : IDisposable, IWriteStringAsync, IWriteBytesAsync
    {
        private const string ZeroLengthError = "Cannot transmit zero-length data.";

        /// <summary>Raised when data is received</summary>
        public event EventHandler<PipeEventArgs> DataReceived;

        /// <summary>Raised when the pipe is closed</summary>
        public event EventHandler<EventArgs> PipeClosed;

        /// <summary>Raised when the pipe is connected</summary>
        public event EventHandler<EventArgs> PipeConnected;


        /// <summary>The <see cref="PipeStream"/> being wrapped</summary>
        protected PipeStream _pipeStream;

        /// <summary>An <see cref="Action"/> used to start the reader</summary>
        protected Action<BasicPipe> _asyncReaderStart;

        protected static Lockable<int> PipeCount = new Lockable<int>(0);

        /// <summary>Initialize a new instance of a <see cref="BasicPipe"/></summary>
        public BasicPipe()
        {
            PipeCount.ExecuteInLock(i => i + 1);
        }

        /// <summary>Close the pipe</summary>
        public void Close()
        {
            if (_pipeStream?.IsConnected ?? false)
                _pipeStream?.WaitForPipeDrain();
            _pipeStream?.Close();
            _pipeStream?.Dispose();
            _pipeStream = null;
        }

        /// <summary>Return the <see cref="PipeStream.IsConnected"/> value of the underlying <see cref="PipeStream"/></summary>
        public bool IsConnected => _pipeStream.IsConnected;

        /// <summary>Calls <see cref="PipeStream.Flush"/> on the underlying <see cref="PipeStream"/></summary>
        public void Flush() => _pipeStream?.Flush();

        /// <summary>The Id of this <see cref="BasicPipe"/></summary>
        public readonly int Id;

        protected void RaisePipeConnected() => PipeConnected?.Invoke(this, EventArgs.Empty);

        protected void StartByteReader(Action<byte[]> packetReceived)
        {
            int intSize = sizeof(int);
            byte[] dataLengthBytes = new byte[intSize];

            _pipeStream.ReadAsync(dataLengthBytes, 0, intSize).ContinueWith(t =>
            {
                int len = t.Result;

                if (len == 0)
                {
                    PipeClosed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    int dataLength = BitConverter.ToInt32(dataLengthBytes, 0);
                    byte[] data = new byte[dataLength];

                    _pipeStream.ReadAsync(data, 0, dataLength).ContinueWith(t2 =>
                    {
                        len = t2.Result;

                        if (len == 0)
                        {
                            PipeClosed?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            packetReceived(data);
                            StartByteReader(packetReceived);
                        }
                    });
                }
            });
        }

        #region Write methods

        /// <summary>Writes a <see cref="string"/> to the <see cref="PipeStream"/></summary>
        /// <param name="str">The string to write</param>
        /// <returns></returns>
        public Task WriteStringAsync(string str) => WriteBytesAsync(
            Encoding.UTF8.GetBytes(str ?? throw new InvalidOperationException(ZeroLengthError)));

        /// <summary>Writes the bytes to the <see cref="PipeStream"/></summary>
        /// <param name="bytes">Array of bytes to write</param>
        /// <returns></returns>
        public Task WriteBytesAsync(byte[] bytes)
        {
            if ((bytes?.Length ?? 0) == 0)
                throw new InvalidOperationException(ZeroLengthError);
            var dataLengthBytes = BitConverter.GetBytes(bytes.Length); // 4 bytes
            var allBytes = dataLengthBytes.Concat(bytes).ToArray();

            return _pipeStream.WriteAsync(allBytes, 0, allBytes.Length);
        }

        #endregion

        #region Read methods

        /// <summary>
        /// Reads an array of bytes, where the first [n] bytes (based on the server's intsize) indicates the number of bytes to read
        /// to complete the packet.
        /// </summary>
        public void StartByteReader() => StartByteReader(bytes => DataReceived?.Invoke(this, new PipeEventArgs(bytes)));

        /// <summary>
        /// Reads an array of bytes, where the first [n] bytes (based on the server's intsize) indicates the number of bytes to read
        /// to complete the packet, and invokes the DataReceived event with a string converted from UTF8 of the byte array.
        /// </summary>
        public void StartStringReader()
        {
            StartByteReader(bytes =>
            {
                string str = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                DataReceived?.Invoke(this, new PipeEventArgs(str));
            });
        }

        #endregion

        public override string ToString() => $"Pipe {Id} " + base.ToString();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BasicPipe() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

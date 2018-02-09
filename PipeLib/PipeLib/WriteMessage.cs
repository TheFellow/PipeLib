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
    public abstract class WriteMessage<T> : IWriteMessage<T>
        where T : class
    {
        protected BasicPipe BasicPipe { get; set; }

        public Action<T> OnMessage { get; set; }

        public Task WriteAsync(T obj)
        {
            var ms = new MemoryStream();
            Serialize(ms, obj);
            return BasicPipe.WriteBytesAsync(ms.ToArray());
        }

        protected virtual void Serialize(MemoryStream ms, T obj)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
        }

        protected void OnDataReceived(object sender, PipeEventArgs e)
        {
            var ms = new MemoryStream(e.Data);
            OnMessage?.Invoke(Deserialize(ms));
        }

        protected virtual T Deserialize(MemoryStream ms)
        {
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(ms);
        }
    }
}

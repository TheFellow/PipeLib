using System;
using System.Threading.Tasks;

namespace PipeLib.Interfaces
{
    public interface IWriteMessage<T>
        where T : class
    {
        Action<T> OnMessage { get; set; }

        Task WriteAsync(T obj);
    }
}
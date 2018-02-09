using System.IO;

namespace PipeLib.Interfaces
{
    public interface ISerializer<T>
        where T : class
    {
        void Serialize(MemoryStream ms, T obj);
        T Deserialize(MemoryStream ms);
    }
}
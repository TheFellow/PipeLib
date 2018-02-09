using System.IO;

namespace PipeLib.Interfaces
{
    /// <summary>
    /// An object which can serialize to and from a memory stream
    /// </summary>
    /// <typeparam name="T">The type being transmitted</typeparam>
    public interface ISerializer<T>
        where T : class
    {
        void Serialize(MemoryStream ms, T obj);
        T Deserialize(MemoryStream ms);
    }
}
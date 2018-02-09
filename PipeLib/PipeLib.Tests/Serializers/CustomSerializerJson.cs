using System.IO;
using PipeLib.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace PipeLib.Tests.Serializers
{
    public class CustomSerializerJson<T> : ISerializer<T>
        where T : class
    {
        public void Serialize(MemoryStream ms, T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            ms.Write(bytes, 0, bytes.Length);
        }

        public T Deserialize(MemoryStream ms)
        {
            string json = Encoding.UTF8.GetString(ms.ToArray());
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}

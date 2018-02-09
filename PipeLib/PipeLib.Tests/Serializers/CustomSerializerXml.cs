using System.IO;
using PipeLib.Interfaces;
using System.Xml.Serialization;

namespace PipeLib.Tests.Serializers
{
    public class CustomSerializerXml<T> : ISerializer<T>
        where T : class
    {
        private XmlSerializer _xmlSerializer = new XmlSerializer(typeof(T));

        public void Serialize(MemoryStream ms, T obj) => _xmlSerializer.Serialize(ms, obj);

        public T Deserialize(MemoryStream ms) => (T)_xmlSerializer.Deserialize(ms);
    }
}

using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Interfaces;
using Newtonsoft.Json;

using static PipeLib.Tests.Constants;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace PipeLib.Tests
{
    [TestClass]
    public class SerializationTests
    {
        public const string PipeName = nameof(SerializableTests);

        private PipeClient<SerializableClass> _pipeClient;
        private PipeServer<SerializableClass> _pipeServer;

        private ManualResetEventSlim _mreConnect = new ManualResetEventSlim();

        [TestInitialize]
        public void TestInitialize()
        {
            _mreConnect.Reset();

            _pipeClient = new PipeClient<SerializableClass>(PipeName);
            _pipeServer = new PipeServer<SerializableClass>(PipeName);

            _pipeClient.OnConnect += () => _mreConnect.Set();
            _pipeClient.Connect(TIMEOUT_MS);

            //JsonConvert.SerializeObject(new object()); // Prime it

            if (!_mreConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_CONNECT);
        }

        [TestMethod]
        public void ISerializerClientSend_Plugin_ExpectedBehavior()
        {
            // Arrange
            var mreSend = new ManualResetEventSlim();
            var expected = new SerializableClass
            {
                IntProp = 123,
                StringProp = "Hello World!",
                BoolField = true,
                ListOfString = new List<string>
                {
                    "First",
                    "Second"
                }
            };

            SerializableClass actual = null;
            _pipeServer.OnMessage = (obj) =>
            {
                actual = obj;
                mreSend.Set();
            };

            var ser = new CustomSerializer<SerializableClass>();
            _pipeClient.Serializer = ser;
            _pipeServer.Serializer = ser;

            // Act
            _pipeClient.WriteAsync(expected);

            if (!mreSend.Wait(TIMEOUT_MS))
                Assert.Fail(TIMEOUT_DATA);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ISerializerServerSend_Plugin_ExpectedBehavior()
        {
            // Arrange
            var mreSend = new ManualResetEventSlim();
            var expected = new SerializableClass
            {
                IntProp = 123,
                StringProp = "Hello World!",
                BoolField = true,
                ListOfString = new List<string>
                {
                    "First",
                    "Second"
                }
            };

            SerializableClass actual = null;
            _pipeClient.OnMessage = (obj) =>
            {
                actual = obj;
                mreSend.Set();
            };

            var ser = new CustomSerializer<SerializableClass>();
            _pipeClient.Serializer = ser;
            _pipeServer.Serializer = ser;

            // Act
            _pipeServer.WriteAsync(expected);

            if (!mreSend.Wait(TIMEOUT_MS))
                Assert.Fail(TIMEOUT_DATA);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }

    public class CustomSerializer<T> : ISerializer<T>
        where T : class
    {
        public T Deserialize(MemoryStream ms)
        {
            string json = Encoding.UTF8.GetString(ms.ToArray());
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Serialize(MemoryStream ms, T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            ms.Write(bytes, 0, bytes.Length);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Tests.Serializers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using static PipeLib.Tests.Constants;

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

            var ser = new CustomSerializerJson<SerializableClass>();

            _pipeClient = new PipeClient<SerializableClass>(PipeName)
            {
                OnConnect = () => _mreConnect.Set(),
                Serializer = ser
            };

            _pipeServer = new PipeServer<SerializableClass>(PipeName)
            {
                Serializer = ser
            };

            _pipeClient.Connect(TIMEOUT_MS);

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

            var ms = new MemoryStream();
            _pipeServer.Serializer.Serialize(ms, expected);
            Debug.WriteLine(Encoding.UTF8.GetString(ms.ToArray()));

            // Act
            _pipeServer.WriteAsync(expected);

            if (!mreSend.Wait(TIMEOUT_MS))
                Assert.Fail(TIMEOUT_DATA);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static PipeLib.Tests.Constants;

namespace PipeLib.Tests
{
    [TestClass]
    public class StringTests
    {
        private ManualResetEventSlim _mreSvrConnect;
        private ManualResetEventSlim _mreCltConnect;
        private PipeServer<string> _pipeServer;
        private PipeClient<string> _pipeClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _mreSvrConnect = new ManualResetEventSlim();
            _mreCltConnect = new ManualResetEventSlim();

            _pipeServer = new PipeServer<string>(nameof(StringTests));
            _pipeClient = new PipeClient<string>(nameof(StringTests));

            _pipeServer.OnConnect += () => _mreSvrConnect.Set();
            _pipeClient.OnConnect += () => _mreCltConnect.Set();

            _pipeClient.Connect(TIMEOUT_MS);

            if (!_mreSvrConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive($"Svr:{TIMEOUT_CONNECT}");
            if (!_mreCltConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive($"Clt:{TIMEOUT_CONNECT}");
        }

        [TestMethod]
        public void Client_WriteAsync_TransmitsStringToServer()
        {
            // Arrange
            var mreMessage = new ManualResetEventSlim();
            string expected = "This is the data to send";
            string actual = null;
            _pipeServer.OnMessage += s =>
            {
                actual = s;
                mreMessage.Set();
            };

            // Act
            _pipeClient.WriteAsync(expected);
            mreMessage.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(mreMessage.IsSet);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Server_WriteAsync_TransmitsStringToClient()
        {
            // Arrange
            var mreMessage = new ManualResetEventSlim();
            string expected = "This is the data to send";
            string actual = null;
            _pipeClient.OnMessage += s =>
            {
                actual = s;
                mreMessage.Set();
            };

            // Act
            _pipeServer.WriteAsync(expected);
            mreMessage.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(mreMessage.IsSet);
            Assert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class SerializableTests
    {
        private ManualResetEventSlim _mreSvrConnect;
        private ManualResetEventSlim _mreCltConnect;
        private PipeServer<SerializableClass> _pipeServer;
        private PipeClient<SerializableClass> _pipeClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _mreSvrConnect = new ManualResetEventSlim();
            _mreCltConnect = new ManualResetEventSlim();

            _pipeServer = new PipeServer<SerializableClass>(nameof(SerializableTests));
            _pipeClient = new PipeClient<SerializableClass>(nameof(SerializableTests));

            _pipeServer.OnConnect += () => _mreSvrConnect.Set();
            _pipeClient.OnConnect += () => _mreCltConnect.Set();

            _pipeClient.Connect(TIMEOUT_MS);

            if (!_mreSvrConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive($"Svr:{TIMEOUT_CONNECT}");
            if (!_mreCltConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive($"Clt:{TIMEOUT_CONNECT}");
        }

        [TestMethod]
        public void Client_WriteAsync_TransmitsDataToServer()
        {
            // Arrange
            var mreMessage = new ManualResetEventSlim();
            SerializableClass expected = GetSerializableTestObject();
            SerializableClass actual = null;
            _pipeServer.OnMessage += s =>
            {
                actual = s;
                mreMessage.Set();
            };

            // Act
            _pipeClient.WriteAsync(expected);
            mreMessage.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(mreMessage.IsSet);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Server_WriteAsync_TransmitsDataToClient()
        {
            // Arrange
            var mreMessage = new ManualResetEventSlim();
            SerializableClass expected = GetSerializableTestObject();
            SerializableClass actual = null;
            _pipeClient.OnMessage += s =>
            {
                actual = s;
                mreMessage.Set();
            };

            // Act
            _pipeServer.WriteAsync(expected);
            mreMessage.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(mreMessage.IsSet);
            Assert.AreEqual(expected, actual);
        }

        private static SerializableClass GetSerializableTestObject()
        {
            return new SerializableClass
            {
                IntProp = 123,
                StringProp = "Hello World!",
                BoolField = true,
                ListOfString = new List<string>
                {
                    "String 1",
                    "String 2"
                }
            };
        }
    }

    // Serializable class
    [Serializable]
    public class SerializableClass
    {
        public int IntProp { get; set; }
        public string StringProp { get; set; }

        public bool BoolField = false;

        public List<string> ListOfString = new List<string>();

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if(obj is SerializableClass testClass)
            {
                if (testClass.IntProp != IntProp) return false;
                if (testClass.StringProp != StringProp) return false;
                if (testClass.BoolField != BoolField) return false;
                if (testClass.ListOfString.Count != ListOfString.Count) return false;
                for (int i = 0; i < ListOfString.Count; i++)
                    if (ListOfString[i] != testClass.ListOfString[i])
                        return false;
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + IntProp.GetHashCode();
                hash = hash * 23 + StringProp.GetHashCode();
                hash = hash * 23 + BoolField.GetHashCode();
                hash = hash * 23 + ListOfString.GetHashCode();
                foreach (var s in ListOfString)
                    hash = hash * 23 + s.GetHashCode();
                return hash;
            }
        }
    }
}

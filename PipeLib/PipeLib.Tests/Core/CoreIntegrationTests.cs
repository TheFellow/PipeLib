using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class CoreIntegrationTests
    {
        private const string pipeName = nameof(CoreIntegrationTests);

        private ServerPipe _stringServer, _binaryServer;
        private ClientPipe _stringClient, _binaryClient;
        private Random rand = new Random();

        [TestInitialize]
        public void TestInitialize()
        {
            var areServer = new ManualResetEventSlim();
            var areClient = new ManualResetEventSlim();

            _stringServer = new ServerPipe(pipeName, p => p.StartStringReader());
            _stringClient = new ClientPipe(".", pipeName, p => p.StartStringReader());
            _binaryServer = new ServerPipe(pipeName, p => p.StartByteReader());
            _binaryClient = new ClientPipe(".", pipeName, p => p.StartByteReader());

            _stringServer.PipeConnected += (o, e) => areServer.Set();
            _stringClient.PipeConnected += (o, e) => areClient.Set();

            _stringClient.Connect();
            _binaryClient.Connect();

            // Wait for the connection
            areClient.Wait();
            areServer.Wait();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _stringServer?.Dispose();
            _stringClient?.Dispose();
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(128)]
        [DataRow(512)]
        [DataRow(1024)]
        [DataRow(8192)] // 8k
        [DataRow(65_536)] // 64k
        [DataRow(524_288)] // 500k
        [DataRow(1_048_576)] // 1MB
        //[DataRow(10_485_760)] // 10MB - Runnable, just slow
        public void ClientServer_WriteData_TransmitsDataSuccessfully(int bytes)
        {
            // Arrange
            var signal = new CountdownEvent(4);

            var sb = new StringBuilder(bytes);

            for (int i = 0; i < bytes; i++)
            {
                sb.Append((char)rand.Next('a', 'z'));
            }

            string stringDataExpected = sb.ToString();
            byte[] binaryDataExpected = Encoding.UTF8.GetBytes(stringDataExpected);

            string stringDataActualServer = string.Empty;
            string stringDataActualClient = string.Empty;
            byte[] binaryDataActualServer = null;
            byte[] binaryDataActualClient = null;

            _stringServer.DataReceived += (o, e) =>
            {
                stringDataActualServer = e.String;
                signal.Signal();
            };
            _stringClient.DataReceived += (o, e) =>
            {
                stringDataActualClient = e.String;
                signal.Signal();
            };
            _binaryServer.DataReceived += (o, e) =>
            {
                binaryDataActualServer = e.Data;
                signal.Signal();
            };
            _binaryClient.DataReceived += (o, e) =>
            {
                binaryDataActualClient = e.Data;
                signal.Signal();
            };

            // Act
            _stringClient.WriteStringAsync(stringDataExpected);
            _stringServer.WriteStringAsync(stringDataExpected);
            _binaryServer.WriteBytesAsync(binaryDataExpected);
            _binaryClient.WriteBytesAsync(binaryDataExpected);

            // Wait for the callbacks
            signal.Wait();

            // Assert
            Assert.AreEqual(stringDataExpected, stringDataActualServer, "String server received incorrect data");
            Assert.AreEqual(stringDataExpected, stringDataActualClient, "String client received incorrect data");
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualServer, "Binary server received incorrect data");
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualClient, "Binary client received incorrect data");
        }
    }
}

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

        private ServerPipe _binaryServer;
        private ClientPipe _binaryClient;
        private Random rand = new Random();

        [TestInitialize]
        public void TestInitialize()
        {
            var connected = new CountdownEvent(4);

            _binaryServer = new ServerPipe(pipeName);
            _binaryClient = new ClientPipe(".", pipeName);

            _binaryServer.PipeConnected += (o, e) => connected.Signal();
            _binaryClient.PipeConnected += (o, e) => connected.Signal();

            _binaryClient.Connect();

            // Wait for the connection
            if (!connected.Wait(50))
                Assert.Inconclusive("Connections were not established in time");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _binaryServer?.Dispose();
            _binaryClient?.Dispose();
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

            byte[] binaryDataActualServer = null;
            byte[] binaryDataActualClient = null;

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
            _binaryServer.WriteBytesAsync(binaryDataExpected);
            _binaryClient.WriteBytesAsync(binaryDataExpected);

            // Wait for the callbacks
            signal.Wait();

            // Assert
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualServer, "Binary server received incorrect data");
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualClient, "Binary client received incorrect data");
        }
    }
}

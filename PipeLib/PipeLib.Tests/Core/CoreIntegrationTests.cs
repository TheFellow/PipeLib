using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

using static PipeLib.Tests.Constants;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class CoreIntegrationTests
    {
        private const string pipeName = nameof(CoreIntegrationTests);
        private ServerPipe _serverPipe;
        private ClientPipe _clientPipe;
        private Random rand = new Random();

        [TestInitialize]
        public void TestInitialize()
        {
            var connected = new CountdownEvent(2);

            _serverPipe = new ServerPipe(pipeName);
            _clientPipe = new ClientPipe(".", pipeName);

            _serverPipe.PipeConnected += (o, e) => connected.Signal();
            _clientPipe.PipeConnected += (o, e) => connected.Signal();

            _clientPipe.Connect(TIMEOUT_MS);

            // Wait for the connection
            if (!connected.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_CONNECT);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _serverPipe?.Dispose();
            _clientPipe?.Dispose();
        }

        [DataTestMethod]
        //[DataRow(0)] // Throws; tested below.
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
            var signal = new CountdownEvent(2);

            var sb = new StringBuilder(bytes);

            for (int i = 0; i < bytes; i++)
            {
                sb.Append((char)rand.Next('a', 'z'));
            }

            string stringDataExpected = sb.ToString();
            byte[] binaryDataExpected = Encoding.UTF8.GetBytes(stringDataExpected);

            byte[] binaryDataActualServer = null;
            byte[] binaryDataActualClient = null;

            _serverPipe.DataReceived += (o, e) =>
            {
                binaryDataActualServer = e.Data;
                signal.Signal();
            };
            _clientPipe.DataReceived += (o, e) =>
            {
                binaryDataActualClient = e.Data;
                signal.Signal();
            };

            // Act
            _serverPipe.WriteBytesAsync(binaryDataExpected);
            _clientPipe.WriteBytesAsync(binaryDataExpected);

            // Wait for the callbacks
            if (!signal.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_CALLBACK);

            // Assert
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualServer, "Binary server received incorrect data");
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualClient, "Binary client received incorrect data");
        }

        [DataTestMethod]
        [DataRow("Client")]
        [DataRow("Server")]
        public void ClientServer_ZeroBytes_ThrowsInvalidOperationException(string clientServer)
        {
            // Arrange
            BasicPipe p;
            if (clientServer == "Client")
            {
                p = new ClientPipe(".", pipeName);
            }
            else
            {
                p = new ServerPipe(pipeName);
            }

            // Act
            Action act = () => p.WriteBytesAsync(new byte[0]);

            // Assert
            Assert.ThrowsException<InvalidOperationException>(act);
        }
    }
}

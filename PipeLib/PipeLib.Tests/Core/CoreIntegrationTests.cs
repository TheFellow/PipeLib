using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class CoreIntegrationTests
    {
        private const string pipeName = "PipeLib.TestPipe";

        private ServerPipe _stringServer, _binaryServer;
        private ClientPipe _stringClient, _binaryClient;
        private Random rand = new Random();

        [TestInitialize]
        public void TestInitialize()
        {
            _stringServer = new ServerPipe(pipeName, p => p.StartStringReader());
            _stringClient = new ClientPipe(".", pipeName, p => p.StartStringReader());
            _binaryServer = new ServerPipe(pipeName, p => p.StartByteReader());
            _binaryClient = new ClientPipe(".", pipeName, p => p.StartByteReader());

            _stringClient.Connect();
            _binaryClient.Connect();

            Thread.Sleep(100); // Yield and give them a second to connect
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

            _stringServer.DataReceived += (o, e) => stringDataActualServer = e.String;
            _stringClient.DataReceived += (o, e) => stringDataActualClient = e.String;
            _binaryServer.DataReceived += (o, e) => binaryDataActualServer = e.Data;
            _binaryClient.DataReceived += (o, e) => binaryDataActualClient = e.Data;

            // Act
            _stringClient.WriteStringAsync(stringDataExpected);
            _stringServer.WriteStringAsync(stringDataExpected);
            _binaryServer.WriteBytesAsync(binaryDataExpected);
            _binaryClient.WriteBytesAsync(binaryDataExpected);

            Debug.WriteLine($"Sample data: {stringDataExpected}");

            Thread.Sleep(100); // Yield

            // Assert
            Assert.AreEqual(stringDataExpected, stringDataActualServer, "String server received incorrect data");
            Assert.AreEqual(stringDataExpected, stringDataActualClient, "String client received incorrect data");
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualServer, "Binary server received incorrect data");
            CollectionAssert.AreEqual(binaryDataExpected, binaryDataActualClient, "Binary client received incorrect data");
        }
    }
}

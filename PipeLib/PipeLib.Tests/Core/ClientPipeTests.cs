using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class ClientPipeTests
    {
        #region Test setup and teardown + helper methods

        private const string pipeName = "PipeLib.TestPipe";

        private ServerPipe _server;
        private ClientPipe _client;
        private bool _onClientConnect;
        private bool _onclientPipeClosed;
        private bool _onClientDataReceived;
        private string _onClientDataReceivedData;

        private ManualResetEventSlim _mreConnect = new ManualResetEventSlim();
        private ManualResetEventSlim _mreDisconnect = new ManualResetEventSlim();
        private ManualResetEventSlim _mreDataReceived = new ManualResetEventSlim();

        [TestInitialize]
        public void TestInitialize()
        {
            _mreConnect.Reset();
            _mreDisconnect.Reset();
            _mreDataReceived.Reset();

            _server = new ServerPipe(pipeName, p => p.StartStringReader());
            _client = new ClientPipe(".", pipeName, p => p.StartStringReader());

            _onClientConnect = false;
            _onclientPipeClosed = false;
            _onClientDataReceived = false;
            _onClientDataReceivedData = string.Empty;

            _client.PipeConnected += OnClientConnect;
            _client.PipeClosed += OnClientPipeClosed;
            _client.DataReceived += OnClientDataReceived;
        }

        private void OnClientDataReceived(object sender, PipeEventArgs e)
        {
            _onClientDataReceived = true;
            _onClientDataReceivedData = e.String;
            _mreDataReceived.Set();
        }

        private void OnClientPipeClosed(object sender, EventArgs e)
        {
            _onclientPipeClosed = true;
            _mreDisconnect.Set();
        }

        private void OnClientConnect(object sender, EventArgs e)
        {
            _onClientConnect = true;
            _mreConnect.Set();
        }

        private void WaitForClientConnection()
        {
            _client.Connect();
            if (!_mreConnect.Wait(50))
                Assert.Inconclusive("The client connection was never established.");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _server?.Dispose();
            _client?.Dispose();
        }

        #endregion

        [TestMethod]
        public void ClientPipe_WhenClientConnected_InvokesPipeConnected()
        {
            // Arrange

            // Act
            _client.Connect();
            _mreConnect.Wait();

            // Assert
            Assert.IsTrue(_onClientConnect);
            Assert.IsTrue(_client.IsConnected);
        }

        [TestMethod]
        public void ClientPipe_WhenServerDisconnects_InvokesPipeClosed()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            _server.Close();
            _mreDisconnect.Wait();

            // Assert
            Assert.IsTrue(_onclientPipeClosed);
        }



        [TestMethod]
        public void ClientPipe_WhenServerSendsData_InvokesDataReceived()
        {
            // Arrange
            WaitForClientConnection();
            string expected = "Data to transmit";

            // Act
            _server.WriteStringAsync(expected);
            _mreDataReceived.Wait();

            // Assert
            Assert.IsTrue(_onClientDataReceived);
            Assert.AreEqual(expected, _onClientDataReceivedData);
        }

        [TestMethod]
        public void ClientPipe_ConnectWithTimeout_ThrowsInvalidOperationException()
        {
            // Arrange
            _client = new ClientPipe(".", pipeName + ".differentpipe", p => p.StartStringReader());

            // Act
            void act()
            {
                _client.Connect(10);
            }

            // Assert
            Assert.ThrowsException<InvalidOperationException>((Action)act);
        }

        [TestMethod]
        public async Task ClientPipe_WriteEmptyString_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _client.WriteStringAsync(string.Empty);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ClientPipe_WriteNullString_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _client.WriteStringAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ClientPipe_WriteEmptyByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _client.WriteBytesAsync(new byte[0]);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ClientPipe_WriteNullByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _client.WriteBytesAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }
    }
}

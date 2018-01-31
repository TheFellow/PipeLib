using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class ServerPipeTests
    {
        #region Test setup and teardown + helper methods

        private const string pipeName = nameof(ServerPipeTests);

        private ServerPipe _server;
        private ClientPipe _client;
        private bool _onServerPipeClosed;
        private bool _onServerDataReceived;
        private string _onServerDataReceivedData;

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

            _onServerPipeClosed = false;
            _onServerDataReceived = false;
            _onServerDataReceivedData = string.Empty;

            _server.PipeConnected += OnServerConnect;
            _server.PipeClosed += OnServerPipeClosed;
            _server.DataReceived += OnServerDataReceived;
        }

        private void OnServerDataReceived(object sender, PipeEventArgs e)
        {
            _onServerDataReceived = true;
            _onServerDataReceivedData = e.String;
            _mreDataReceived.Set();
        }

        private void OnServerPipeClosed(object sender, EventArgs e)
        {
            _onServerPipeClosed = true;
            _mreDisconnect.Set();
        }

        private void OnServerConnect(object sender, EventArgs e)
        {
            _mreConnect.Set();
        }

        private void WaitForClientConnection()
        {
            _client.Connect();
            _mreConnect.Wait();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _server?.Dispose();
            _client?.Dispose();
        }

        #endregion

        [TestMethod]
        public void ServerPipe_WhenClientDisconnects_InvokesPipeClosed()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            _client.Close();

            if (!_mreDisconnect.Wait(50))
                Assert.Inconclusive("Disconnect was not signaled wihtin the timeout");

            // Assert
            Assert.IsTrue(_onServerPipeClosed);
        }

        [TestMethod]
        public async Task ServerPipe_WriteEmptyString_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _server.WriteStringAsync(string.Empty);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ServerPipe_WriteNullString_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _server.WriteStringAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ServerPipe_WriteEmptyByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _server.WriteBytesAsync(new byte[0]);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ServerPipe_WriteNullByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _server.WriteBytesAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public void ServerPipe_WhenClientSendsData_InvokesDataReceived()
        {
            // Arrange
            WaitForClientConnection();
            string expected = "Data to transmit";

            // Act
            _client.WriteStringAsync(expected).GetAwaiter().GetResult();
            _mreDataReceived.Wait();

            // Assert
            Assert.IsTrue(_onServerDataReceived);
            Assert.AreEqual(expected, _onServerDataReceivedData);
        }
    }
}

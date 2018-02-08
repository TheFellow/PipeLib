using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

using static PipeLib.Tests.Constants;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class ServerPipeTests
    {
        #region Test setup and teardown + helper methods

        private const string pipeName = nameof(ServerPipeTests);

        private ServerPipe _serverPipe;
        private ClientPipe _clientPipe;
        private bool _onServerPipeClosed;
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

            _serverPipe = new ServerPipe(pipeName);
            _clientPipe = new ClientPipe(".", pipeName);

            _onServerPipeClosed = false;
            _onServerDataReceivedData = string.Empty;

            _serverPipe.PipeConnected += OnServerConnect;
            _serverPipe.PipeClosed += OnServerPipeClosed;
            _serverPipe.DataReceived += OnServerDataReceived;
        }

        private void OnServerDataReceived(object sender, PipeEventArgs e)
        {
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
            _clientPipe.Connect();
            _mreConnect.Wait();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _serverPipe?.Dispose();
            _clientPipe?.Dispose();
        }

        #endregion

        [TestMethod]
        public void ServerPipe_WhenClientDisconnects_InvokesPipeClosed()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            _clientPipe.Close();

            if (!_mreDisconnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_DISCONNECT);

            // Assert
            Assert.IsTrue(_onServerPipeClosed);
        }

        [TestMethod]
        public async Task ServerPipe_WriteEmptyByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _serverPipe.WriteBytesAsync(new byte[0]);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ServerPipe_WriteNullByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _serverPipe.WriteBytesAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public void ServerPipe_WhenClientSendsData_InvokesDataReceived()
        {
            // Arrange
            WaitForClientConnection();
            string expected = "Data to transmit";
            byte[] bytes = Encoding.UTF8.GetBytes(expected);
        
            // Act
            _clientPipe.WriteBytesAsync(bytes).GetAwaiter().GetResult();
            bool dataReceived = _mreDataReceived.Wait(TIMEOUT_MS);
        
            // Assert
            Assert.IsTrue(dataReceived);
            Assert.AreEqual(expected, _onServerDataReceivedData);
        }
    }
}

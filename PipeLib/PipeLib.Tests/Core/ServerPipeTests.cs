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
        private byte[] _onServerDataReceivedData;

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

            _onServerDataReceivedData = null;

            _serverPipe.PipeConnected += (o, e) => _mreConnect.Set();
            _serverPipe.PipeClosed += (o, e) => _mreDisconnect.Set();
            _serverPipe.DataReceived += (o, e) =>
            {
                _onServerDataReceivedData = e.Data;
                _mreDataReceived.Set();
            };
        }

        private void WaitForConnect()
        {
            _clientPipe.ConnectAsync();
            if (!_mreConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_CONNECT);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _serverPipe?.Dispose();
            _clientPipe?.Dispose();
        }

        #endregion

        [TestMethod]
        public void ServerPipe_WhenClientConnects_InvokesPipeConnected()
        {
            // Arrange

            // Act
            _clientPipe.ConnectAsync();
            if (!_mreConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_CONNECT);

            // Assert
            Assert.IsTrue(_mreConnect.IsSet, TIMEOUT_CONNECT);
            Assert.IsTrue(_serverPipe.IsConnected);
        }

        [TestMethod]
        public void ServerPipe_WhenClientDisconnects_InvokesPipeClosed()
        {
            // Arrange
            WaitForConnect();

            // Act
            _clientPipe.Close();

            if (!_mreDisconnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_DISCONNECT);

            // Assert
            Assert.IsTrue(_mreDisconnect.IsSet, TIMEOUT_DISCONNECT);
        }

        [TestMethod]
        public void ServerPipe_WhenClientSendsData_InvokesDataReceived()
        {
            // Arrange
            WaitForConnect();
            string expectedString = "Data to transmit";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedString);

            // Act
            _clientPipe.WriteBytesAsync(expectedBytes);
            _mreDataReceived.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(_mreDataReceived.IsSet, TIMEOUT_DATA);
            Assert.AreEqual(expectedString, Encoding.UTF8.GetString(_onServerDataReceivedData));
        }

        [TestMethod]
        public async Task ServerPipe_WriteEmptyByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForConnect();

            // Act
            Task func() => _serverPipe.WriteBytesAsync(new byte[0]);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ServerPipe_WriteNullByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForConnect();

            // Act
            Task func() => _serverPipe.WriteBytesAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }
    }
}

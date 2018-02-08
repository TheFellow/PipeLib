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
    public class ClientPipeTests
    {
        #region Test setup and teardown + helper methods

        private string pipeName = typeof(ClientPipeTests).FullName;

        private ServerPipe _serverPipe;
        private ClientPipe _clientPipe;
        private byte[] _onClientDataReceivedData;

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

            _onClientDataReceivedData = null;

            _clientPipe.PipeConnected += (o, e) => _mreConnect.Set();
            _clientPipe.PipeClosed += (o, e) => _mreDisconnect.Set();
            _clientPipe.DataReceived += (o, e) =>
            {
                _onClientDataReceivedData = e.Data;
                _mreDataReceived.Set();
            };
        }

        private void WaitForClientConnection()
        {
            _clientPipe.ConnectAsync(TIMEOUT_MS);
            if (!_mreConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive("The client connection was never established.");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _serverPipe?.Dispose();
            _clientPipe?.Dispose();
        }

        #endregion

        [TestMethod]
        public void ClientPipe_WhenClientConnected_InvokesPipeConnected()
        {
            // Arrange

            // Act
            _clientPipe.ConnectAsync();
            if (!_mreConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive(TIMEOUT_CONNECT);

            // Assert
            Assert.IsTrue(_mreConnect.IsSet, TIMEOUT_CONNECT);
            Assert.IsTrue(_clientPipe.IsConnected);
        }

        [TestMethod]
        public void ClientPipe_WhenServerDisconnects_InvokesPipeClosed()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            _serverPipe.Close();
            _mreDisconnect.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(_mreDisconnect.IsSet, TIMEOUT_DISCONNECT);
        }

        [TestMethod]
        public void ClientPipe_WhenServerSendsData_InvokesDataReceived()
        {
            // Arrange
            WaitForClientConnection();
            string expectedString = "Data to transmit";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedString);

            // Act
            _serverPipe.WriteBytesAsync(expectedBytes);
            _mreDataReceived.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(_mreDataReceived.IsSet, TIMEOUT_DATA);
            Assert.AreEqual(expectedString, Encoding.UTF8.GetString(_onClientDataReceivedData));
        }

        [TestMethod]
        public void ClientPipe_ConnectWithTimeout_ThrowsInvalidOperationException()
        {
            // Arrange
            _clientPipe = new ClientPipe(".", pipeName + ".differentpipe");

            // Act
            void act()
            {
                _clientPipe.Connect(10);
            }

            // Assert
            Assert.ThrowsException<InvalidOperationException>((Action)act);
        }

        [TestMethod]
        public async Task ClientPipe_WriteEmptyByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _clientPipe.WriteBytesAsync(new byte[0]);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        [TestMethod]
        public async Task ClientPipe_WriteNullByteArray_ThrowsInvalidOperationException()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            Task func() => _clientPipe.WriteBytesAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }
    }
}

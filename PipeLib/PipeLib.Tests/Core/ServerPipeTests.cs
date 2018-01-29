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

        private const string pipeName = "PipeLib.TestPipe";
        private const int testDelay = 50; // 50ms

        private ServerPipe _server;
        private ClientPipe _client;
        private bool _onServerConnect;
        private bool _onServerPipeClosed;
        private bool _onServerDataReceived;
        private string _onServerDataReceivedData;

        [TestInitialize]
        public void TestInitialize()
        {
            _server = new ServerPipe(pipeName, p => p.StartStringReader());
            _client = new ClientPipe(".", pipeName, p => p.StartStringReader());

            _onServerConnect = false;
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
        }

        private void OnServerPipeClosed(object sender, EventArgs e)
        {
            _onServerPipeClosed = true;
        }

        private void OnServerConnect(object sender, EventArgs e)
        {
            _onServerConnect = true;
        }

        private void WaitForClientConnection()
        {
            _client.Connect();
            Thread.Sleep(testDelay); // Yield

            if (!_onServerConnect)
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
        public void ServerPipe_WhenClientConnects_InvokesPipeConnected()
        {
            // Arrange

            // Act
            _client.Connect();
            Thread.Sleep(testDelay); // Yield

            // Assert
            Assert.IsTrue(_onServerConnect);
        }

        [TestMethod]
        public void ServerPipe_WhenClientDisconnects_InvokesPipeClosed()
        {
            // Arrange
            WaitForClientConnection();

            // Act
            _client.Close();

            Thread.Sleep(testDelay); // Yield

            // Assert
            Assert.IsTrue(_onServerPipeClosed);
        }

        

        [TestMethod]
        public void ServerPipe_WhenClientSendsData_InvokesDataReceived()
        {
            // Arrange
            WaitForClientConnection();
            string expected = "Data to transmit";

            // Act
            _client.WriteStringAsync(expected).GetAwaiter().GetResult();
            Thread.Sleep(testDelay); // Yield

            // Assert
            Assert.IsTrue(_onServerDataReceived);
            Assert.AreEqual(expected, _onServerDataReceivedData);
        }
    }
}

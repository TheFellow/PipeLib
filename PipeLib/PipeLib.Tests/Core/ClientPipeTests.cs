﻿using System;
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
        private const int testDelay = 50; // 50ms

        private ServerPipe _server;
        private ClientPipe _client;
        private bool _onClientConnect;
        private bool _onclientPipeClosed;
        private bool _onClientDataReceived;
        private string _onClientDataReceivedData;

        [TestInitialize]
        public void TestInitialize()
        {
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
        }

        private void OnClientPipeClosed(object sender, EventArgs e)
        {
            _onclientPipeClosed = true;
        }

        private void OnClientConnect(object sender, EventArgs e)
        {
            _onClientConnect = true;
        }

        private void WaitForClientConnection()
        {
            _client.Connect();
            Thread.Sleep(testDelay); // Yield

            if (!_onClientConnect)
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
            Thread.Sleep(testDelay); // Yield

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

            Thread.Sleep(testDelay); // Yield

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
            _server.WriteStringAsync(expected).GetAwaiter().GetResult();
            Thread.Sleep(testDelay); // Yield

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
                Thread.Sleep(10 + testDelay);
            }

            // Assert
            Assert.ThrowsException<InvalidOperationException>((Action)act);
        }
    }
}

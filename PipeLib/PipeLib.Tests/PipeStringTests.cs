using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PipeLib.Tests
{
    [TestClass]
    public class PipeStringTests
    {
        private PipeStringServer _server;
        private PipeStringClient _client;

        [TestInitialize]
        public void TestInitialize()
        {
            _server = new PipeStringServer(nameof(PipeStringTests));
            _client = new PipeStringClient(nameof(PipeStringTests));

            _client.Connect();
        }

        [TestMethod]
        public void PipeStringServerAndClient_ConnectAndTransmit()
        {
            // Arrange
            string expected = "This is some data to send";
            int serverId = 0;
            int clientId = 0;
            string clientActual = null;
            string serverActual = null;

            _server.MessageReceived = (id, str) =>
            {
                clientId = id;
                clientActual = str;
            };

            _client.MessageReceived = (id, str) =>
            {
                serverId = id;
                serverActual = str;
            };

            // Act
            _server.Send(expected);
            _client.Send(expected);

            Thread.Sleep(100); // Yield

            // Assert
            Assert.AreEqual(expected, serverActual, "Server error");
            Assert.AreEqual(expected, clientActual, "Client error");
        }
    }
}

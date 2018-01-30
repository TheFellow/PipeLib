using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PipeLib.Tests
{
    [TestClass]
    public class StringClientServerTests
    {
        private const string PipeName = nameof(StringClientServerTests);
        private StringPipeServer _server;
        private StringPipeClient _client;

        private bool _serverIsConnected;
        private string _serverReceived;

        private bool _clientIsConnected;
        private string _clientReceived;


        [TestInitialize]
        public void TestInitialize()
        {
            _server = new StringPipeServer(PipeName)
            {
                PipeConnected = () => _serverIsConnected = true,
                PipeClosed = () => _serverIsConnected = false,
                MessageReceived = (id, str) => _serverReceived = str
            };
            _client = new StringPipeClient(PipeName)
            {
                PipeConnected = () => _clientIsConnected = true,
                PipeClosed = () => _clientIsConnected = false,
                MessageReceived = (id, str) => _clientReceived = str
            };

            _serverReceived = null;
            _serverIsConnected = false;

            _clientReceived = null;
            _clientIsConnected = false;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _server?.Dispose();
            _client?.Dispose();
        }

        [TestMethod]
        public async Task StringPipe_SendAndReceive()
        {
            // Arrange
            _client.Connect();
            string expected = nameof(StringPipe_SendAndReceive);

            // Act
            await Task.WhenAll(new Task[] { _client.WriteStringAsync(expected), _server.WriteStringAsync(expected) });

            // Assert
            Assert.IsTrue(_clientIsConnected);
            Assert.IsTrue(_serverIsConnected);
            Assert.AreEqual(expected, _serverReceived);
            Assert.AreEqual(expected, _clientReceived);
        }
    }
}

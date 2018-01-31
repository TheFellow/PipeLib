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
        private bool _serverIsConnected;
        private string _serverReceived;
        private ManualResetEventSlim _areServer;

        private StringPipeClient _client;
        private bool _clientIsConnected;
        private string _clientReceived;
        private ManualResetEventSlim _areClient;


        [TestInitialize]
        public void TestInitialize()
        {
            _server = new StringPipeServer(PipeName)
            {
                PipeConnected = () => _serverIsConnected = true,
                PipeClosed = () => _serverIsConnected = false,
                MessageReceived = (id, str) =>
                {
                    _serverReceived = str;
                    _areServer.Set();
                }
            };
            _client = new StringPipeClient(PipeName)
            {
                PipeConnected = () => _clientIsConnected = true,
                PipeClosed = () => _clientIsConnected = false,
                MessageReceived = (id, str) =>
                {
                    _clientReceived = str;
                    _areClient.Set();
                }
            };

            _serverReceived = null;
            _serverIsConnected = false;
            _areServer = new ManualResetEventSlim();

            _clientReceived = null;
            _clientIsConnected = false;
            _areClient = new ManualResetEventSlim();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _server?.Dispose();
            _client?.Dispose();
        }

        private void WaitForCallbacks()
        {
            _areServer.Wait();
            _areClient.Wait();
        }

        [TestMethod]
        public void StringPipe_SendAndReceive()
        {
            // Arrange
            _client.Connect();
            string expected = nameof(StringPipe_SendAndReceive);

            // Act
            _client.WriteStringAsync(expected);
            _server.WriteStringAsync(expected);

            WaitForCallbacks();

            // Assert
            Assert.IsTrue(_clientIsConnected);
            Assert.IsTrue(_serverIsConnected);
            Assert.AreEqual(expected, _serverReceived);
            Assert.AreEqual(expected, _clientReceived);
        }
    }
}

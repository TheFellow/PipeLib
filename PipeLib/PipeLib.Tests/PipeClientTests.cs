using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static PipeLib.Tests.Constants;

namespace PipeLib.Tests
{
    [TestClass]
    public class PipeClientTests
    {
        private ManualResetEventSlim _mreSvrConnect;
        private ManualResetEventSlim _mreCltConnect;
        private PipeServer<string> _pipeServer;
        private PipeClient<string> _pipeClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _mreSvrConnect = new ManualResetEventSlim();
            _mreCltConnect = new ManualResetEventSlim();

            _pipeServer = new PipeServer<string>(nameof(PipeClientTests));
            _pipeClient = new PipeClient<string>(nameof(PipeClientTests));

            _pipeServer.OnConnect += () => _mreSvrConnect.Set();
            _pipeClient.OnConnect += () => _mreCltConnect.Set();

            _pipeClient.ConnectAsync();

            if (!_mreSvrConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive($"Svr:{TIMEOUT_CONNECT}");
            if (!_mreCltConnect.Wait(TIMEOUT_MS))
                Assert.Inconclusive($"Clt:{TIMEOUT_CONNECT}");
        }

        [TestMethod]
        public void Client_WriteAsync_TransmitsDataToServer()
        {
            // Arrange
            var mreMessage = new ManualResetEventSlim();
            string expected = "This is the data to send";
            string actual = null;
            _pipeServer.OnMessage += s =>
            {
                actual = s;
                mreMessage.Set();
            };

            // Act
            _pipeClient.WriteAsync(expected);
            mreMessage.Wait(TIMEOUT_MS);

            // Assert
            Assert.IsTrue(mreMessage.IsSet);
            Assert.AreEqual(expected, actual);
        }
    }
}

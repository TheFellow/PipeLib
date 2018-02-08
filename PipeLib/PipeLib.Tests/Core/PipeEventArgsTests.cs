using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class PipeEventArgsTests
    {
        [TestMethod]
        public void Ctor_WithByteArrayArgument_HasByteData()
        {
            // Arrange
            string seedString = "Hello World!";
            byte[] expected = System.Text.Encoding.UTF8.GetBytes(seedString);
            int len = expected.Length;

            // Act
            var args = new PipeEventArgs(expected);

            // Assert
            CollectionAssert.AreEqual(expected, args.Data);
            Assert.AreEqual("byte[12]", args.ToString());
            Assert.AreEqual(len, args.Length);
        }
    }
}

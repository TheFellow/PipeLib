using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeLib.Core;

namespace PipeLib.Tests.Core
{
    [TestClass]
    public class PipeEventArgsTests
    {
        [TestMethod]
        public void Ctor_WithStringArgument_HasStringData()
        {
            // Arrange
            string expected = "Hello World!";
            int len = expected.Length;

            // Act
            var args = new PipeEventArgs(expected);

            // Assert
            Assert.AreEqual(expected, args.String);
            Assert.AreEqual(expected, args.ToString());
            Assert.AreEqual(len, args.Length);
            Assert.IsTrue(args.IsString);
            Assert.IsFalse(args.IsBytes);
            Assert.IsNull(args.Data);
        }

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
            Assert.IsFalse(args.IsString);
            Assert.IsTrue(args.IsBytes);
            Assert.IsNull(args.String);
        }
    }
}

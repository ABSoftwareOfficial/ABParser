using ABParse;
using ABParse.Tests.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABParse.Tests
{
    [TestClass]
    public class ABParserTests
    {
        [TestMethod]
        public void GeneralTest()
        {
            // Arrange
            MyParser parser = new MyParser();

            // Act
            parser.Start("Hello world!.Anotherone,");

            // Assert
            //Assert.AreEqual();
        }
    }
}

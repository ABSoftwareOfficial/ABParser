using ABParse.Tests.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests
{
    [TestClass]
    public class FeatureTests
    {
        NextTokenFeatureParser nextTokenParser;
        LogicalOrderFeatureParser logicalOrderParser;

        [TestMethod, TestCategory("Extra Features")]
        public void ValidNextTokens()
        {
            nextTokenParser = new NextTokenFeatureParser();
            var NextTokens = new ABParserToken[]
            {
                nextTokenParser.Tokens[1],
                null
            };
            nextTokenParser.Start("hello.world,great");

            CollectionAssert.AreEqual(nextTokenParser.NextToken.ToArray(), NextTokens);
        }

        public void StartLogicalOrderParser(string text)
        {
            logicalOrderParser = new LogicalOrderFeatureParser();
            logicalOrderParser.Start(text);
        }

        [TestMethod, TestCategory("Extra Features")]
        public void ValidLogicalOrder_OCP()
        {
            // OCP = On Character Processed
            string text = "hello1world";
            StartLogicalOrderParser(text);

            // Check if a start was reached.
            Assert.IsTrue(logicalOrderParser.Events[0].Type == LogicalOrderEventType.Start);

            // Check if the characters are correct.
            for (int i = 1; i < 6; i++)
            {
                Assert.IsTrue(logicalOrderParser.Events[i].Type == LogicalOrderEventType.Character);
                Assert.IsTrue(logicalOrderParser.Events[i].Character == text[i - 1]);
            }

            // Check if there was a "before token" encountered next.
            Assert.IsTrue(logicalOrderParser.Events[6].Type == LogicalOrderEventType.BeforeTokenProcessed);
            Assert.IsTrue(logicalOrderParser.Events[6].Token.Name == "1");

            // Next should be the "OnTokenProcessed".
            Assert.IsTrue(logicalOrderParser.Events[7].Type == LogicalOrderEventType.OnTokenProcessed);
            Assert.IsTrue(logicalOrderParser.Events[7].Token.Name == "1");

            // After that, should be more characters.
            for (int i = 8; i < 12; i++)
            {
                Assert.IsTrue(logicalOrderParser.Events[i].Type == LogicalOrderEventType.Character);
                Assert.IsTrue(logicalOrderParser.Events[i].Character == text[i - 2]);
            }

            // Finally, there should be an end.
            Assert.IsTrue(logicalOrderParser.Events[12].Type == LogicalOrderEventType.End);

        }
    }
}

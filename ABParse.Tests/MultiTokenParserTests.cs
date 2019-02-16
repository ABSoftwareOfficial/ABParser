using System;
using System.Collections.Generic;
using ABParse.Tests.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABParse.Tests
{
    [TestClass]
    public class MultiTokenParserTests
    {
        MultiTokenParser parser;

        public string[] Leading = new string[] { "first", "second", "third", "fourth", "fifth" };
        public string[] Trailing = new string[] { "second", "third", "fourth", "fifth", "sixth" };
        public string[] Names = new string[] { "EXCLAMATION", "COMMA", "DOT", "LEFT", "RIGHT" };
        public int[] StartsEndsAndCurrentLocations = new int[] { 5, 12, 18, 25, 31 };

        public string TestString;

        public void ExecuteParser()
        {
            parser = new MultiTokenParser();
            parser.Start("first!second,third.fourth<fifth>sixth");
        }

        [TestMethod, TestCategory("Multi Token Parser")]
        public void ValidLeading()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Leads.ToArray(), Leading);
        }

        [TestMethod, TestCategory("Multi Token Parser")]
        public void ValidTrailing()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Trails.ToArray(), Trailing);
        }

        [TestMethod, TestCategory("Multi Token Parser")]
        public void ValidNames()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Names.ToArray(), Names);
        }

        [TestMethod, TestCategory("Multi Token Parser")]
        public void ValidStarts()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Starts.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Multi Token Parser")]
        public void ValidEnds()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Ends.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Multi Token Parser")]
        public void ValidCurrentLocations()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.CurrentLocations.ToArray(), StartsEndsAndCurrentLocations);
        }
    }
}

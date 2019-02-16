using System;
using ABParse.Tests.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABParse.Tests
{
    [TestClass]
    public class WhitespaceFeatureParserTests
    {
        WhitespaceFeatureParser parser;

        public string[] Leading = new string[] { "first", "second" };
        public string[] Trailing = new string[] { "second", "third" };
        public string[] Names = new string[] { "RETURN", "NEWLINE" };
        public int[] StartsEndsAndCurrentLocations = new int[] { 5, 12 };


        public void ExecuteParser()
        {
            parser = new WhitespaceFeatureParser();
            parser.Start("first\rsecond\nthird");
        }

        [TestMethod, TestCategory("Feature - Ignore Whitespace")]
        public void ValidLeading()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Leads.ToArray(), Leading);
        }

        [TestMethod, TestCategory("Feature - Ignore Whitespace")]
        public void ValidTrailing()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Trails.ToArray(), Trailing);
        }

        [TestMethod, TestCategory("Feature - Ignore Whitespace")]
        public void ValidNames()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Names.ToArray(), Names);
        }

        [TestMethod, TestCategory("Feature - Ignore Whitespace")]
        public void ValidStarts()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Starts.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Feature - Ignore Whitespace")]
        public void ValidEnds()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Ends.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Feature - Ignore Whitespace")]
        public void ValidCurrentLocations()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.CurrentLocations.ToArray(), StartsEndsAndCurrentLocations);
        }
    }
}

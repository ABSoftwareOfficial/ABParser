using System;
using ABParse.Tests.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABParse.Tests
{
    [TestClass]
    public class EscapingFeatureTests
    {
        MyParser parser;

        public string[] Leading = new string[] { "hello.world" };
        public string[] Trailing = new string[] { "great" };
        public string[] Names = new string[] { "COMMA" };
        public int[] StartsEndsAndCurrentLocations = new int[] { 12 };

        public void ExecuteParser()
        {
            parser = new MyParser();
            parser.Start("hello\\.world,great");
        }

        [TestMethod, TestCategory("Feature - Escaping")]
        public void ValidLeading()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Leads.ToArray(), Leading);
        }

        [TestMethod, TestCategory("Feature - Escaping")]
        public void ValidTrailing()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Trails.ToArray(), Trailing);
        }

        [TestMethod, TestCategory("Feature - Escaping")]
        public void ValidNames()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Names.ToArray(), Names);
        }

        [TestMethod, TestCategory("Feature - Escaping")]
        public void ValidStarts()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Starts.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Feature - Escaping")]
        public void ValidEnds()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Ends.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Feature - Escaping")]
        public void ValidCurrentLocations()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.CurrentLocations.ToArray(), StartsEndsAndCurrentLocations);
        }
    }
}

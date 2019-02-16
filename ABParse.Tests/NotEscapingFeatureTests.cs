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
    public class NotEscapingFeatureTests
    {
        NotEscapedParser parser;

        public string[] Leading = new string[] { "hello\\", "world" };
        public string[] Trailing = new string[] { "world", "great" };
        public string[] Names = new string[] { "DOT", "COMMA" };
        public int[] StartsEndsAndCurrentLocations = new int[] { 6, 12 };

        public void ExecuteParser()
        {
            parser = new NotEscapedParser();
            parser.Start("hello\\.world,great");
        }

        [TestMethod, TestCategory("Feature - Not Escaping")]
        public void ValidLeading()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Leads.ToArray(), Leading);
        }

        [TestMethod, TestCategory("Feature - Not Escaping")]
        public void ValidTrailing()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Trails.ToArray(), Trailing);
        }

        [TestMethod, TestCategory("Feature - Not Escaping")]
        public void ValidNames()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Names.ToArray(), Names);
        }

        [TestMethod, TestCategory("Feature - Not Escaping")]
        public void ValidStarts()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Starts.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Feature - Not Escaping")]
        public void ValidEnds()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Ends.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Feature - Not Escaping")]
        public void ValidCurrentLocations()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.CurrentLocations.ToArray(), StartsEndsAndCurrentLocations);
        }
    }
}

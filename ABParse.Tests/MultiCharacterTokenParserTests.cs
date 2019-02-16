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
    public class MultiCharacterTokenParserTests
    {
        MultiCharacterTokenParser parser;

        public string[] Leading = new string[] { "", " ", " ", " Demo() { " };
        public string[] Trailing = new string[] { " ", " ", " Demo() { ", " }"};
        public string[] Names = new string[] { "public", "static", "void", "another" };
        public int[] Starts = new int[] { 0, 7, 14, 28 };
        public int[] Ends = new int[] { 5, 12, 17, 34 };

        public string TestString;

        public void ExecuteParser()
        {
            parser = new MultiCharacterTokenParser();
            parser.Start("public static void Demo() { another }");
        }

        [TestMethod, TestCategory("Multi Character Parser")]
        public void ValidLeading()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Leads.ToArray(), Leading);
        }

        [TestMethod, TestCategory("Multi Character Parser")]
        public void ValidTrailing()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Trails.ToArray(), Trailing);
        }

        [TestMethod, TestCategory("Multi Character Parser")]
        public void ValidNames()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Names.ToArray(), Names);
        }

        [TestMethod, TestCategory("Multi Character Parser")]
        public void ValidStarts()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Starts.ToArray(), Starts);
        }

        [TestMethod, TestCategory("Multi Character Parser")]
        public void ValidEnds()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Ends.ToArray(), Ends);
        }

        [TestMethod, TestCategory("Multi Character Parser")]
        public void ValidCurrentLocations()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.CurrentLocations.ToArray(), Ends);
        }
    }
}

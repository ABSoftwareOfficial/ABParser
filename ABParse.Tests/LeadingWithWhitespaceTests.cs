using ABParse.Tests.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ABParse.Tests
{
    [TestClass]
    public class MyParserTests
    {
        // I THINK THIS WAS FIXED IN AN UPDATE.
        //[TestMethod, TestCategory("MyParser (Very Simple)")]
        //public void aIGNORE_THIS_TEST()
        //{
        //    // For some reason the first test that is run has an inaccurate time on it/runs really slow.
        //    // So, to make the times fairer - this test is just a dummy to essentially soak up the slow time on the first one. Seriously, Microsoft, fix it (I think THEY need better tests - pun intended).

        //    // Arrange

        //    var str = "*";

        //    // Act

        //    str += "*******************************";

        //    // Assert

        //    Assert.AreEqual("********************************", str);
        //}

        MyParser parser;

        public string[] Leading = new string[] { "Hello world!", "Anotherone" };
        public string[] Trailing = new string[] { "Anotherone", "Ending" };
        public string[] Names = new string[] { "DOT", "COMMA" };
        public int[] StartsEndsAndCurrentLocations = new int[] { 12, 23 };

        public string TestString;

        public void ExecuteParser()
        {
            parser = new MyParser();
            parser.Start("Hello world!.Anotherone,Ending");
        }

        [TestMethod, TestCategory("Leading With Whitespace Parser")]
        public void ValidLeading()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Leads.ToArray(), Leading);
        }

        [TestMethod, TestCategory("Leading With Whitespace Parser")]
        public void ValidTrailing()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Trails.ToArray(), Trailing);
        }

        [TestMethod, TestCategory("Leading With Whitespace Parser")]
        public void ValidNames()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Names.ToArray(), Names);
        }

        [TestMethod, TestCategory("Leading With Whitespace Parser")]
        public void ValidStarts()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Starts.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Leading With Whitespace Parser")]
        public void ValidEnds()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.Ends.ToArray(), StartsEndsAndCurrentLocations);
        }

        [TestMethod, TestCategory("Leading With Whitespace Parser")]
        public void ValidCurrentLocations()
        {
            ExecuteParser();
            CollectionAssert.AreEqual(parser.CurrentLocations.ToArray(), StartsEndsAndCurrentLocations);
        }
    }
}

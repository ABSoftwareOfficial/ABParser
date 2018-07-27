using ABParse.Tests.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ABParse.Tests
{
    [TestClass]
    public class ABParserTests
    {
        [TestMethod]
        public void aIGNORE_THIS_TEST()
        {
            // For some reason the first test that is run has an inaccurate time on it/runs really slow.
            // So, to make the times fairer - this test is just a dummy to essentially soak up the slow time on the first one. Seriously, Microsoft, fix it (I think THEY need better tests - pun intended).

            // Arrange

            var str = "*";

            // Act

            str += "*******************************";

            // Assert

            Assert.AreEqual("********************************", str);
        }
        [TestMethod]
        public void MyParser_TwoDots_CorrectLeadingValues()
        {
            // Arrange
            MyParser parser = new MyParser();

            // Act
            var timer = Stopwatch.StartNew();

            parser.Start("Hello world!.Anotherone,Ending");
            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);

            // Assert
            CollectionAssert.AreEqual(new string[] { "Hello world!", "Anotherone" }, parser.Leads.ToArray());
        }

        [TestMethod]
        public void MyParser_TwoDots_CorrectTrailingValues()
        {
            // Arrange
            MyParser parser = new MyParser();

            // Act
            var timer = Stopwatch.StartNew();

            parser.Start("Hello world!.Anotherone,Ending");

            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);

            // Assert
            CollectionAssert.AreEqual(new string[] { "Anotherone", "Ending" }, parser.Trails.ToArray());
        }

        [TestMethod]
        public void MyParser_TwoDotsNoTrailingEnd_CorrectLeadingValues()
        {
            // Arrange
            MyParser parser = new MyParser();

            // Act
            var timer = Stopwatch.StartNew();

            parser.Start("Hello world!.Anotherone,");

            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);

            // Assert
            CollectionAssert.AreEqual(new string[] { "Hello world!", "Anotherone" }, parser.Leads.ToArray());
        }

        [TestMethod]
        public void MyParser_TwoDotsNoTrailingEnd_CorrectTrailingValues()
        {
            // Arrange
            MyParser parser = new MyParser();

            // Act
            var timer = Stopwatch.StartNew();

            parser.Start("Hello world!.Anotherone,");

            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);

            // Assert
            CollectionAssert.AreEqual(new string[] { "Anotherone", "" }, parser.Trails.ToArray());
        }
    }
}

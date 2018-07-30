using ABParse.Tests.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Example();
            Console.ReadLine();
            // Arrange
            MyParser parser = new MyParser();

            // Act
            parser.Start("Hello world!.Anotherone,");
            //parser.Start("Hello world!.Anotherone,plz..evenmore,,justwork.WOW,ITWORKS,,..FINALPART");

            // Assert
            parser.Trails.ForEach((itm) => Console.WriteLine(itm));

            //Assert.AreEqual(new List<string>() { "Hello world!", "Anotherone" }, parser.Leads);
            Console.ReadLine();

            PerformanceTests.RunAll();

            Console.ReadLine();
        }

        public static void Example()
        {
            //var parser = new DemoParser();
            //parser.Start("hello!world?another");

            var parser = new JsonParser();
            parser.Start("{\"hello\":[32, 478],\"cool\":{\"another\":[\"Come on!\", \"You can do it :p\"]}}");

            Console.WriteLine("Done!");
        }
    }
}

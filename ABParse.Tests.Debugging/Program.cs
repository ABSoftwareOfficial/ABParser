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
            // Arrange
            MyParser parser = new MyParser();

            // Act
            parser.Start("Hello world!.Anotherone,plz..evenmore,,justwork.WOW,ITWORKS,,..FINALPART");

            Console.ReadLine();
        }
    }
}

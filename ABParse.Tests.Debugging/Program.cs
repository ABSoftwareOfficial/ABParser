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
            var demo = new DemoParser();
            demo.Start("hello\\.world,great");

            Console.ReadLine();
        }
    }
}

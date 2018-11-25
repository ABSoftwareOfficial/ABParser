using ABParse.Tests.Debugging.JSONVideo;
using ABParse.Tests.Debugging.Other;
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
            var demo = new JsonParser();
            demo.Start(@"
{
	""hello"":""world"",
	""object"": 
	{
		""good"": 351,
		""anotherobj"":
		{
			""yay"": true,
			""nay"": false
		}
	},
	""array"":
	[
		{""anotherobj"":""great"",""ok"":11616},
		{""excellent"":""ok""  ,  ""done"":""ok""}
	]
}
");

            Console.ReadLine();
        }
    }
}

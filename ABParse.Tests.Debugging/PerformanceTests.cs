using ABParse.Tests.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging
{
    public static class PerformanceTests
    {
        public const int EXECUTION_TIMES = 100;

        public static long SimpleTestSpeed()
        {
            var timer = Stopwatch.StartNew();
            var parser = new DemoParser();
            parser.Start("Hello world!.Anotherone,");
            timer.Stop();

            return timer.ElapsedTicks;
        }

        public static void RunAll()
        {
            var totalTime = 0L;
            for (int i = 0; i < EXECUTION_TIMES; i++)
                totalTime += SimpleTestSpeed();

            totalTime = totalTime / EXECUTION_TIMES;

            Console.WriteLine(totalTime);
        }
    }
}

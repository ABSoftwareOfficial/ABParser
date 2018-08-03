using ABParse.Tests.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging
{
    internal class SingleTokenParser : ABParser
    {
        public SingleTokenParser()
        {
            Tokens = new ObservableCollection<ABParserToken>()
            {
                new ABParserToken("Example", 'k')
            };
        }
    }

    internal class SingleCharParser : ABParser
    {
        public override bool NotifyCharacterProcessed => true;
        public long ElapsedTicks;
        public Stopwatch timer;

        protected override void OnCharacterProcessed(char ch)
        {
            if (timer == null)
            {
                timer = Stopwatch.StartNew();
            } else {
                timer.Stop();

                ElapsedTicks = timer.ElapsedTicks;
                Stop();
            }
        }
    }

    internal class SimpleJSONParser : ABParser
    {
        public SimpleJSONParser()
        {
            Tokens = new ObservableCollection<ABParserToken>()
            {
                new ABParserToken(nameof(JsonParserTokens.ObjectStart), '{'),
                new ABParserToken(nameof(JsonParserTokens.ObjectEnd), '}'),
                new ABParserToken(nameof(JsonParserTokens.ArrayStart), '['),
                new ABParserToken(nameof(JsonParserTokens.ArrayEnd), ']'),
                new ABParserToken(nameof(JsonParserTokens.String), '"'),
                new ABParserToken(nameof(JsonParserTokens.PairSeperator), ':'),
                new ABParserToken(nameof(JsonParserTokens.ItemSeperator), ',')
            };
        }
    }

    public static class PerformanceTests
    {
        public const int EXECUTION_TIMES = 100;

        public static long SearchSpeed()
        {
            var timer = Stopwatch.StartNew();
            var parser = new ABParser();

            parser.Start("{\"hello\":\"world!\",\"another\":[37, 431]}");
            timer.Stop();

            return timer.ElapsedTicks;
        }

        public static long SingleTokenSpeed()
        {
            var timer = Stopwatch.StartNew();
            var parser = new SingleTokenParser();

            parser.Start("allkden");
            timer.Stop();

            return timer.ElapsedTicks;
        }

        public static long ProcessCharSpeed()
        {
            var parser = new SingleCharParser();

            parser.Start("abcd");

            return parser.ElapsedTicks;
        }

        public static long JsonParserNoExtraCode()
        {
            var timer = Stopwatch.StartNew();
            var parser = new SimpleJSONParser();

            parser.Start("{\"hello\":\"world!\",\"another\":[37, 431]}");
            timer.Stop();

            return timer.ElapsedTicks;
        }

        public static long ToListOnTokens()
        {
            var timer = Stopwatch.StartNew();

            new JsonParser().Tokens.ToList();

            timer.Stop();

            return timer.ElapsedTicks;
        }

        public static long SimpleTestSpeed()
        {
            var timer = Stopwatch.StartNew();
            var parser = new JsonParser();

            parser.Start("{\"hello\":[32, 478],\"cool\":{\"another\":[\"Come on!\", \"You can do it :p\"]}}");
            timer.Stop();

            return timer.ElapsedTicks;
        }

        public static void RunAll()
        {
            // Total Times
            var totalTime1 = 0L;
            var totalTime2 = 0L;
            var totalTime3 = 0L;
            var totalTime4 = 0L;
            var totalTime5 = 0L;

            for (int i = 0; i < EXECUTION_TIMES; i++)
                totalTime1 += SearchSpeed();

            for (int i = 0; i < EXECUTION_TIMES; i++)
                totalTime2 += SingleTokenSpeed();

            for (int i = 0; i < EXECUTION_TIMES; i++)
                totalTime3 += ProcessCharSpeed();

            for (int i = 0; i < EXECUTION_TIMES; i++)
                totalTime4 += JsonParserNoExtraCode();

            for (int i = 0; i < EXECUTION_TIMES; i++)
                totalTime5 += SimpleTestSpeed();

            totalTime1 = totalTime1 / EXECUTION_TIMES;
            totalTime2 = totalTime2 / EXECUTION_TIMES;
            totalTime3 = totalTime3 / EXECUTION_TIMES;
            totalTime4 = totalTime4 / EXECUTION_TIMES;
            totalTime5 = totalTime5 / EXECUTION_TIMES;

            Console.WriteLine("SEARCH SPEED: " + new TimeSpan(totalTime1).ToString() + "(" + new TimeSpan(totalTime1).TotalMilliseconds + "ms)");
            Console.WriteLine("SINGLE TOKEN PARSE: " + new TimeSpan(totalTime2).ToString() + "(" + new TimeSpan(totalTime2).TotalMilliseconds + "ms)");
            Console.WriteLine("PROCESSCHAR SPEED: " + new TimeSpan(totalTime3).ToString() + "(" + new TimeSpan(totalTime3).TotalMilliseconds + "ms)");
            Console.WriteLine("JSON STRING WITH TOKENS: " + new TimeSpan(totalTime4).ToString() + "(" + new TimeSpan(totalTime4).TotalMilliseconds + "ms)");
            Console.WriteLine("COMPLETE JSON PARSE: " + new TimeSpan(totalTime5).ToString() + "(" + new TimeSpan(totalTime5).TotalMilliseconds + "ms)");
        }
    }
}

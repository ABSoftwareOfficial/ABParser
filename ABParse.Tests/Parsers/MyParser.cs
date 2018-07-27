using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class MyParser : ABParser
    {
        public override ABParserToken[] Rules
        {
            get
            {
                return new ABParserToken[]
                {
                    new ABParserToken("DOT", "."),
                    new ABParserToken("COMMA", ","),
                };
            }
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            base.OnTokenProcessed(e);

            Console.WriteLine(e.Token.Name + ": " + e.Leading);
            Console.WriteLine(e.Token.Name + ": " + e.Trailing);
        }
    }
}

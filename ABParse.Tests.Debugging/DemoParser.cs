using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging
{
    public class DemoParser : ABParser
    {
        public DemoParser()
        {
            Tokens = new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
            {
                new ABParserToken("DOT", '.'),
                new ABParserToken("COMMA", ','),
                new ABParserToken("STRING", '"')
            };
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            Console.WriteLine(e.Token.Name + ": " + e.Leading);
            Console.WriteLine(e.Token.Name + ": " + e.Trailing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging
{
    public class DemoParser : ABParser
    {

        public DemoParser()
        {
            Tokens = new ObservableCollection<ABParserToken>()
            {
                new ABParserToken("EXCLAMATION", '!'),
                new ABParserToken("QUESTION", '?')
            };
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            base.OnTokenProcessed(e);

            Console.WriteLine(e.Token.Name + ": " + e.Leading);
            Console.WriteLine(e.Token.Name + ": " + e.Trailing);
        }
    }
}

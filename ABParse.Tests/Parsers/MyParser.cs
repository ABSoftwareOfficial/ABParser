using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class MyParser : ABParser
    {
        public List<string> Leads = new List<string>();
        public List<string> Trails = new List<string>();
        
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

            Leads.Add(e.Leading);
            Trails.Add(e.Trailing);

            ////Console.WriteLine(e.Token.Name + ": " + e.Leading);
            ////Console.WriteLine(e.Token.Name + ": " + e.Trailing);
        }
    }
}

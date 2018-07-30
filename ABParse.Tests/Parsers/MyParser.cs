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
        
        public override List<ABParserToken> Rules
        {
            get
            {
                return new List<ABParserToken>()
                {
                    new ABParserToken("DOT", "."),
                    new ABParserToken("COMMA", ","),
                };
            }
        }

        //public override bool NotifyCharacterProcessed { get { return true; } }

        protected override void OnCharacterProcessed(char ch)
        {
            base.OnCharacterProcessed(ch);

            //Console.WriteLine(ch);
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            base.OnTokenProcessed(e);

            //Console.WriteLine(e.StartLocation);
            //Console.WriteLine(e.EndLocation);

            //Leads.Add(e.Leading);
            //Trails.Add(e.Trailing);

            //Console.WriteLine(e.Token.Name + ": " + e.Leading);
            //Console.WriteLine(e.Token.Name + ": " + e.Trailing);
        }
    }
}

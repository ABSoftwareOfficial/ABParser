using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class NextTokenFeatureParser : MyParser
    {
        public List<ABParserToken> NextToken = new List<ABParserToken>();

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            NextToken.Add(e.NextToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class WhitespaceFeatureParser : TestingBaseParser
    {
        public override bool IgnoreWhitespace => false;

        public WhitespaceFeatureParser() : base(new ObservableCollection<ABParserToken>()
        {
            new ABParserToken("RETURN", '\r'),
            new ABParserToken("NEWLINE", '\n')
        })
        { }
    }
}

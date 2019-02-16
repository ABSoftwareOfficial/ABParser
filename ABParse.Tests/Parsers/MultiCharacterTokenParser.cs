using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class MultiCharacterTokenParser : TestingBaseParser
    {
        public MultiCharacterTokenParser() : base(new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
        {
            new ABParserToken("public"),
            new ABParserToken("static"),
            new ABParserToken("void"),
            new ABParserToken("another")
        })
        { }
    }
}

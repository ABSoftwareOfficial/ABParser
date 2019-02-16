using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class MultiTokenParser : TestingBaseParser
    {
        
        public MultiTokenParser() : base(new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
            {
                new ABParserToken("COMMA", ','),
                new ABParserToken("DOT", '.'),
                new ABParserToken("EXCLAMATION", '!'),
                new ABParserToken("LEFT", '<'),
                new ABParserToken("RIGHT", '>'),
            })
        { }
    }
}

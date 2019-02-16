using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class MyParser : TestingBaseParser
    {
        public MyParser() : base(new ObservableCollection<ABParserToken>()
            {
                new ABParserToken("DOT", "."),
                new ABParserToken("COMMA", ","),
            })
        { }

    }
}

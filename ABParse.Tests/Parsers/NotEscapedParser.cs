using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    public class NotEscapedParser : MyParser
    {
        public override bool EscapeTokens => false;
    }
}

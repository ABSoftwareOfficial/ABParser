using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse
{
    /// <summary>
    /// The data the ABParser finds at a certain point summed up into a class.
    /// </summary>
    public class TokenProcessedEventArgs
    {
        private char[] _leading;
        private char[] _trailing;

        private string _cacheLeading = "";
        private string _cacheTrailing = "";

        /// <summary>
        /// The actual token.
        /// </summary>
        public ABParserToken Token { get; internal set; }

        /// <summary>
        /// The next token.
        /// </summary>
        public ABParserToken NextToken { get; internal set; }
        
        /// <summary>
        /// The location that this token has started at.
        /// </summary>
        public int StartLocation { get; internal set; }

        /// <summary>
        /// The location that this token has ended at.
        /// </summary>
        public int EndLocation { get; internal set; }

        /// <summary>
        /// The text leading up to this token - from the last token.
        /// </summary>
        public string Leading
        {
            get
            {
                if (_cacheLeading == "")
                    _cacheLeading = new string(_leading);

                return _cacheLeading;
            }
        }

        /// <summary>
        /// The text after this token - up to the next token.
        /// </summary>
        public string Trailing
        {
            get
            {
                if (_cacheTrailing == "")
                    _cacheTrailing = new string(_trailing);

                return _cacheTrailing;
            }
        }

        public char[] LeadingCharacterArr
        {
            get => _leading;
        }

        public char[] TrailingCharacterArr
        {
            get => _trailing;
        }

        public TokenProcessedEventArgs(ABParserToken token, ABParserToken nexttoken, int start, int end, char[] leading, char[] trailing)
        {
            // Set the start and end locations.
            StartLocation = start;
            EndLocation = end;

            // Set the leading and trailing text.
            _leading = leading;
            _trailing = trailing;

            if (nexttoken == null)
                NextToken = new ABParserToken("", '\0');
            else
                NextToken = nexttoken;

            // Set the token.
            Token = token;
        }
    }
}

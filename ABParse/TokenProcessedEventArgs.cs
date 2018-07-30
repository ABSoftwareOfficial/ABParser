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
                    for (int i = 0; i < _leading.Length; i++)
                        _cacheLeading += _leading[i];

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
                    for (int i = 0; i < _trailing.Length; i++)
                        _cacheTrailing += _trailing[i];

                return _cacheTrailing;
            }
        }

        public TokenProcessedEventArgs(ABParserToken token, int start, int end, List<char> leading, List<char> trailing)
        {
            // Set the start and end locations.
            StartLocation = start;
            EndLocation = end;

            // Set the leading and trailing text.
            _leading = leading.ToArray();
            _trailing = trailing.ToArray();

            // Set the token.
            Token = token;
        }
    }
}

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
        private StringBuilder _leading;
        private StringBuilder _trailing;

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
        /// The text leading up to this token.
        /// </summary>
        public string Leading
        {
            get { return _leading.ToString(); }
        }

        /// <summary>
        /// The text after this token.
        /// </summary>
        public string Trailing
        {
            get { return _trailing.ToString(); }
        }

        public TokenProcessedEventArgs(ABParserToken token, int start, int end, StringBuilder leading, StringBuilder trailing)
        {
            // Set the start and end locations.
            StartLocation = start;
            EndLocation = end;

            // Set the leading and trailing text.
            _leading = leading;
            _trailing = trailing;

            // Set the token.
            Token = token;
        }
    }
}

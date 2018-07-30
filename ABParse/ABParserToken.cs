using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse
{
    /// <summary>
    /// A certain token that the parser should recognize.
    /// </summary>
    public class ABParserToken
    {
        /// <summary>
        /// The name of this token, e.g. NextItem
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The actual text that should be recognized.
        /// </summary>
        public char[] Token { get; set; }

        // All the constructors for creating this parser:

        /// <summary>
        /// Create a new token - based on the string given.
        /// </summary>
        /// <param name="name">The name of this token, e.g. NextItem</param>
        /// <param name="token">The token (as a string)</param>
        public ABParserToken(string name, string token)
        {
            Name = name;
            Token = ABParser.ToCharArray(token);
        }

        /// <summary>
        /// <para>Create a new token - based on the StringBuilder given.</para> This should have a higher performance instead of the string one as at the core this class uses StringBuilders, for performance (just like the actual <see cref="ABParser"/> class)
        /// </summary>
        /// <param name="name">The name of this token, e.g. NextItem</param>
        /// <param name="token">The token (as a StringBuilder)</param>
        public ABParserToken(string name, StringBuilder token)
        {
            Name = name;
            token.CopyTo(0, Token, 0, token.Length);
        }

        /// <summary>
        /// <para>Create a new token - based on the StringBuilder given.</para> This may have a lower performance instead of the string/StringBuilder one as the characters have to be converted to a StringBuilder.
        /// </summary>
        /// <param name="name">The name of this piece of token, e.g. NextItem</param>
        /// <param name="token">The characters of this token</param>
        public ABParserToken(string name, params char[] token)
        {
            Name = name;
            Token = token;
        }
    }
}

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

        /// <summary>
        /// Makes both the name and token the same, from a single string.
        /// </summary>
        /// <param name="nameAndToken"></param>
        public ABParserToken(string nameAndToken)
        {
            Name = nameAndToken;
            Token = nameAndToken.ToCharArray();
        }

        /// <summary>
        /// Create a new token - based on the string given.
        /// </summary>
        /// <param name="name">The name of this token, e.g. NextItem</param>
        /// <param name="token">The token (as a string)</param>
        public ABParserToken(string name, string token)
        {
            Name = name;
            Token = token.ToCharArray();
        }

        /// <summary>
        /// <para>Create a new token - based on the StringBuilder given.</para> This will have a slightly lower performance since ABParser uses character arrays behind the scenes (which is essentially a StringBuilder is)
        /// </summary>
        /// <param name="name">The name of this token, e.g. NextItem</param>
        /// <param name="token">The token (as a StringBuilder)</param>
        public ABParserToken(string name, StringBuilder token)
        {
            Name = name;
            token.CopyTo(0, Token, 0, token.Length);
        }

        /// <summary>
        /// <para>Create a new token - based on the character given.</para> This will have the highest performance since the parser uses strings in the background.
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

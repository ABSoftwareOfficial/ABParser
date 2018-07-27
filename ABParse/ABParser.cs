using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse
{
    /// <summary>
    /// A class designed to help a string be parsed based on a set of "tokens" it should recognize.
    /// </summary>
    public class ABParser
    {
        /// <summary>
        /// <para>The public location sometimes isn't accurate - because, the parser tends to skip ahead to get trailing things - this is the actual location it has made it to.</para>
        /// <para>Whereas, the public one just says where it is based on what part the user will be on - however, behind the scenes... it may have skipped ahead a bit to get more data.</para>
        /// </summary>
        private int _currentLocation;

        /// <summary>
        /// The text that has been built up so far.
        /// </summary>
        private StringBuilder _textBuildup = new StringBuilder();

        /// <summary>
        /// Can either be leading/trailing - primary and secondary have their roles swapped to save processing power copying memory over.
        /// </summary>
        private StringBuilder _primaryBuildUp = new StringBuilder();

        /// <summary>
        /// Can either be leading/trailing - primary and secondary have their roles swapped to save processing power copying memory over.
        /// </summary>
        private StringBuilder _secondaryBuildUp = new StringBuilder();

        /// <summary>
        /// Whether the primary build up is the leading text.
        /// </summary>
        private bool _usePrimary = true;

        /// <summary>
        /// Whether we should toggle _usePrimary.
        /// </summary>
        private bool _togglePrimary;

        /// <summary>
        /// The start location of the current token.
        /// </summary>
        private int _tokenStart;

        /// <summary>
        /// The end location of the current token.
        /// </summary>
        private int _tokenEnd;

        /// <summary>
        /// Whether we've found a token that EXACTLY matches our current build up.
        /// </summary>
        private bool _foundExactToken;

        /// <summary>
        /// The token that EXACTLY matches our current build up...
        /// </summary>
        private ABParserToken _exactToken;

        /// <summary>
        /// The token so far, when processing a character.
        /// </summary>
        private List<ABParserToken> _builtUp;

        /// <summary>
        /// Queue up the next token - the parser will always skip ahead to the next one to get the Trailing text.
        /// </summary>
        private ABParserToken _queue;

        /// <summary>
        /// Whether the parser is actually parsing the text or not.
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// The location the parser has reached.
        /// </summary>
        public int CurrentLocation { get; private set; }

        /// <summary>
        /// All the rules.
        /// </summary>
        public virtual ABParserToken[] Rules { get; set; }

        /// <summary>
        /// The main string.
        /// </summary>
        public StringBuilder Text { get; set; }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(string text)
        {
            Text = new StringBuilder(text);
            PerformStart();
        }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(StringBuilder text)
        {
            Text = text;
            PerformStart();
        }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(params char[] text)
        {
            Text = new StringBuilder();

            // Convert the array of characters to a string
            for (int i = 0; i < text.Length; i++)
                Text.Append(text[i]);

            // Start the code
            PerformStart();
        }

        /// <summary>
        /// Actually does the start...
        /// </summary>
        private void PerformStart()
        {
            // If we are ALREADY in the process of parsing, don't bother.
            if (IsProcessing)
                return;

            // Make sure we are parsing now
            IsProcessing = true;

            // Reset the location
            CurrentLocation = 0;

            // Get the start of every token and place it into a variable for easy access.
            //_cacheStartChars = new List<char>();

            //for (int i = 0; i < Rules.Length; i++)
            //    _cacheStartChars.Add(Rules[i].Token[0]);

            // Set the built up tokens back to default - ALL of the tokens.
            _builtUp = Rules.ToList();

            // Loop through every character - this is the very heart of this parser.
            for (_currentLocation = 0; _currentLocation < Text.Length; _currentLocation++)
            {
                // Call the event when for processing a character.
                OnCharacterProcessed();

                // Stop executing if we aren't processing anymore.
                if (!IsProcessing)
                    break;
            }

            // Make sure we definitely complete the queued up token.
            PerformToken(null);
        }

        /// <summary>
        /// When a token is found.
        /// </summary>
        /// <param name="token"></param>
        private void PerformToken(ABParserToken token)
        {
            // If there is already a token queued up, process it.
            if (_queue != null)
            {
                // Toggle _usePrimary if needed - because it was toggled to let it build up the trailing.
                _usePrimary = (_usePrimary) ? false : true;

                // Make sure the "CurrentLocation" variable is actually the location of this token.
                CurrentLocation = _tokenStart;

                // Call the overrideable method.
                OnTokenProcessed(new TokenProcessedEventArgs(_queue, _tokenStart, _tokenEnd,
                    (_usePrimary) ? _primaryBuildUp : _secondaryBuildUp, (_usePrimary) ? _secondaryBuildUp : _primaryBuildUp));

                // Toggle _usePrimary if it wasn't done when this token was actually found.
                if (!_togglePrimary)
                    _usePrimary = (_usePrimary) ? false : true;
                //// Set the _queue to null.
                //_queue = null;
            }

            // If a token has been passed to this method, queue it up.
            if (token != null)
            {
                // If we should change "_usePrimary"
                _togglePrimary = (_togglePrimary) ? false : true;

                // Toggle _usePrimary, if needed.
                if (_togglePrimary)
                    _usePrimary = (_usePrimary) ? false : true;

                _foundExactToken = false;

                // Don't do anything else, except set the queue if this is the first one.
                if (_queue == null)
                {
                    _queue = token;
                    return;
                }

                // Queue up this token.
                _queue = token;

                // Clear whatever build up is the OLD leading.
                if (_usePrimary)
                    _primaryBuildUp.Clear();
                else
                    _secondaryBuildUp.Clear();
            }
        }

        protected virtual void OnCharacterProcessed()
        {
            // Actually process this character - basically everything is called from that one method.
            ProcessChar();

            // Trigger the event.
            // TODO: Event Support.
        }

        protected virtual void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            // This method is literally designed to be overrided... So, we only need to trigger the event.

            // Trigger the event.
            // TODO: More event support.
        }

        /// <summary>
        /// The heart of the parser - processes a character.
        /// </summary>
        /// <param name="builtUp"></param>
        private void ProcessChar()
        {
            // We add all the tokens that still fit, then place that into the main "_builtUp" array.
            var newBuildupTokens = new List<ABParserToken>();

            // Add the current character to the built up item.
            _textBuildup.Append(Text[_currentLocation]);

            // Check which token the current build-up text fits under, keeping track of how many are left.
            for (int i = 0; i < _builtUp.Count; i++)

                if (_builtUp[i].Token.ToString() == _textBuildup.ToString())
                {
                    // Say that we've found a token with the exact same text as the build-up so far.
                    _foundExactToken = true;
                    _exactToken = _builtUp[i];

                    // Set the _tokenEnd variable.
                    _tokenEnd = _currentLocation;

                    // We'll leave it to deal with this on the next character.
                    return;
                }

                // If the current builtUp one we are testing does actually start with our current built-up string.
                else if (_builtUp[i].Token.ToString().StartsWith(_textBuildup.ToString()))
                {
                    newBuildupTokens.Add(_builtUp[i]);

                    // Just in case this turns out to be the one we are looking for - store this as the start location, if it hasn't already been set.
                    if (_tokenStart == 0)
                        _tokenStart = _currentLocation;
                }

            // The possible tokens this could be.
            _builtUp = newBuildupTokens;

            // If we didn't find one, clean out the build-up.
            if (_builtUp.Count == 0)
            {
                // Check if we actually found a perfect match along the way.
                if (_foundExactToken)
                    PerformToken(_exactToken);

                // Add the character to the current build up (it could actually be leading or trailing).
                if (_usePrimary)
                    _primaryBuildUp.Append(Text[_currentLocation]);
                else
                    _secondaryBuildUp.Append(Text[_currentLocation]);

                // Clear all the built up text - we're done with this one..
                _textBuildup.Clear();

                // Reset the _builtUp array.
                _builtUp = Rules.ToList();
            }

            // Finally, if we still haven't found the exact token, just add to the current build up for leading.
            else if (_builtUp.Count > 1)
                if (_usePrimary)
                    _primaryBuildUp.Append(Text[_currentLocation]);
                else
                    _secondaryBuildUp.Append(Text[_currentLocation]);
        }
    }
}

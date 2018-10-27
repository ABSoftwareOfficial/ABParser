using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ABParse
{
    /// <summary>
    /// Represents the PossibleTokenStart has been set/unset.
    /// </summary>
    public delegate void ABParserPossibleTokenStartChanged();

    /// <summary>
    /// A class designed to help a string be parsed based on a set of "tokens" it should recognize.
    /// </summary>
    public class ABParser
    {
        #region Private Variables
        /// <summary>
        /// <para>The public location sometimes isn't accurate - because, the parser tends to skip ahead to get trailing things - this is the actual location it has made it to.</para>
        /// <para>Whereas, the public one just says where it is based on what part the user will be on - however, behind the scenes... it may have skipped ahead a bit to get more data.</para>
        /// </summary>
        private int _currentLocation;

        /// <summary>
        /// All the tokens.
        /// </summary>
        private ObservableCollection<ABParserToken> _tokens = new ObservableCollection<ABParserToken>();

        /// <summary>
        /// The text that has been built up so far.
        /// </summary>
        private List<char> _textBuildup = new List<char>();

        /// <summary>
        /// Can either be leading/trailing - primary and secondary have their roles swapped to save processing power copying memory over.
        /// </summary>
        private List<char> _primaryBuildUp = new List<char>();

        /// <summary>
        /// Can either be leading/trailing - primary and secondary have their roles swapped to save processing power copying memory over.
        /// </summary>
        private List<char> _secondaryBuildUp = new List<char>();

        /// <summary>
        /// Whether the primary build up is the leading text.
        /// </summary>
        private bool _usePrimary = true;

        /// <summary>
        /// Whether we should toggle _usePrimary.
        /// </summary>
        private bool _togglePrimary;

        /// <summary>
        /// The start location of the queued token.
        /// </summary>
        private int _queueTokenStart;

        /// <summary>
        /// The end location of the queued token.
        /// </summary>
        private int _queueTokenEnd;

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
        /// Whether the next character is escaped or not.
        /// </summary>
        private bool _charIsEscaped = false;

        /// <summary>
        /// The first characters of each token - for performance.
        /// </summary>
        private char[] _firstTokensChars = new char[0];

        /// <summary>
        /// All of the tokens that fit within the token limit...
        /// </summary>
        private ABParserToken[] _validTokens;

        /// <summary>
        /// Only tokens starting with these characters are allowed.
        /// </summary>
        private ObservableCollection<char[]> _tokenLimit = new ObservableCollection<char[]>();

        /// <summary>
        /// The possible start of the current token.
        /// </summary>
        private int _possibleTokenStart = -1;

        #endregion

        #region Public Variables

        /// <summary>
        /// Whether the parser should bother to call the <see cref="OnCharacterProcessed"/> method - as well as the event. (WILL RUN SLOWER)
        /// </summary>
        public virtual bool NotifyCharacterProcessed { get { return false; } }

        /// <summary>
        /// Whether the parser should ignore character followed by the <see cref="EscapeCharacter"/>.
        /// </summary>
        public virtual bool EscapeTokens { get; set; } = true;

        /// <summary>
        /// The character the parser should use to escape tokens - if <see cref="EscapeTokens"/> is true. NOTE: YOUR TOKENS CAN'T CONTAIN THIS CHARACTER.
        /// </summary>
        public virtual char EscapeCharacter { get; set; } = '\\';

        /// <summary>
        /// This will tell the parser whether it should ignore whitespace or not - if this is true, the parser may run MUCH faster... But, you won't be able to use whitespace in tokens.
        /// </summary>
        public virtual bool IgnoreWhitespace { get; set; } = true;

        /// <summary>
        /// Whether the parser is actually parsing the text or not.
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// The location the parser has reached.
        /// </summary>
        public int CurrentLocation { get; private set; }

        /// <summary>
        /// The possible start location of the next token.
        /// </summary>
        public int PossibleTokenStart
        {
            get { return _possibleTokenStart; }
            set
            {
                _possibleTokenStart = value;

                PossibleTokenStartChanged?.Invoke();
            }
        }

        /// <summary>
        /// The end location of the current token.
        /// </summary>
        public int PossibleTokenEnd;

        public ABParserPossibleTokenStartChanged PossibleTokenStartChanged;

        /// <summary>
        /// Don't let the CurrentLocation variable be changed anywhere except the start/end methods.
        /// </summary>
        public bool UseMoveForwardToChangePosition { get; set; }

        /// <summary>
        /// All the rules.
        /// </summary>
        public ObservableCollection<ABParserToken> Tokens
        {
            get { return _tokens; }
            set
            {
                _tokens = value;
                ManageStartChars();
                Tokens.CollectionChanged += Tokens_CollectionChanged;
            }
        }

        /// <summary>
        /// Only tokens starting with these characters are allowed. NOTE: If this array is empty, all tokens will be allowed - use <see cref="ObservableCollection{T}.ClearItems()"/> to clear it.
        /// </summary>
        public virtual ObservableCollection<char[]> TokenLimit
        {
            get { return _tokenLimit; }
            set
            {
                _tokenLimit = value;
                ManageStartChars();
                TokenLimit.CollectionChanged += Tokens_CollectionChanged;
            }
        }

        /// <summary>
        /// Whether the current token limit affects the tokens AFTER the NEXT token (the trailing of the next token) - can be quite useful for strings when only want to limit the contents.
        /// </summary>
        public virtual bool LimitAffectsNextTrailing { get; set; } = true;

        /// <summary>
        /// The main string.
        /// </summary>
        public char[] Text { get; set; }
        #endregion

        #region Unmanaged Code - Text Management
        public unsafe static char[] ToCharArray(string str)
        {
            // Create an array of characters.
            var chArray = new char[str.Length];

            // Fix a pointer to this new array.
            fixed (char* fixedPointer = chArray)
            {
                // Create a non-fixed pointer to the same location as the fixed pointer.
                var pointer = fixedPointer;

                // Add the characters from the string to the char array!
                for (int i = 0; i < chArray.Length; i++)
                    *(pointer++) = str[i];
            }

            // Return the array.
            return chArray;
        }

        internal unsafe static bool StartsWith(char[] arr, char[] contents)
        {
            // Get two (fixed) pointers for both char arrays.
            fixed (char* fixedPointer = arr, fixedPointer2 = contents)
            {
                var pointer = fixedPointer;
                var pointer2 = fixedPointer2;

                // Go through each character in the start array - if any character doesn't line up, return false.
                for (int i = 0; i < arr.Length; i++)
                    if (contents.Length > i)
                    {
                        if (*(pointer++) != *(pointer2++))
                            return false;
                    }
                    else break;
            }

            // If it made it here - return true.
            return true;
        }

        internal unsafe static bool StartsWith(char[] arr, List<char> contents)
        {
            // Get two (fixed) pointers for both char arrays.
            fixed (char* fixedPointer = arr)
            {
                var pointer = fixedPointer;

                // Go through each character in the start array - if any character doesn't line up, return false.
                for (int i = 0; i < arr.Length; i++)
                    if (contents.Count > i)
                    {
                        if (*(pointer++) != contents[i])
                            return false;
                    }
                    else break;
            }

            // If it made it here - return true.
            return true;
        }

        internal unsafe void ManageStartChars()
        {
            var chArray = new char[Tokens.Count];
            var tkArray = new ABParserToken[Tokens.Count];

            fixed (char* resultPointer = chArray)
            {
                var pointer = resultPointer;

                // If there are limits, get all the first characters - EXCLUDING the non-limited ones.
                if (_tokenLimit.Count > 0)
                {

                    // Go through every rule.
                    for (int i = 0; i < Tokens.Count; i++)
                        if (Tokens[i].Token.Length > 0)

                            // Go through every token limit - only allowing the ones that fit under them.
                            for (int j = 0; j < _tokenLimit.Count; j++)
                                if (StartsWith(Tokens[i].Token, _tokenLimit[j]))
                                {
                                    // Add it.
                                    tkArray[pointer - resultPointer] = Tokens[i];
                                    *(pointer++) = Tokens[i].Token[0];

                                    // We're done with this item.
                                    break;
                                }
                }

                // Otherwise, if there's no limit... Just make an array based on all the rules.
                else
                    for (int i = 0; i < Tokens.Count; i++)
                        if (Tokens[i].Token.Length > 0)
                        {

                            // Add it.
                            tkArray[pointer - resultPointer] = Tokens[i];
                            *(pointer++) = Tokens[i].Token[0];
                        }

                // The actual length we want in the final destination.
                var count = pointer - resultPointer;

                // Now, create the new character array - but, only with the actual length we want.
                _firstTokensChars = new char[count];
                _validTokens = new ABParserToken[count];

                fixed (char* destinationPointer = _firstTokensChars)
                {
                    var fPointer = destinationPointer;
                    // Reset the pointer back to the beginning.
                    pointer = resultPointer;

                    // Copy them over to the _firstTokensChars and _validTokens array, which is the final destination.
                    for (int i = 0; i < count; i++)
                    {
                        *(fPointer++) = *(pointer++);
                        _validTokens[i] = tkArray[i];
                    }
                }
            }

        }
        #endregion

        #region Managed Code
        public ABParser()
        {
            Tokens.CollectionChanged += Tokens_CollectionChanged;
            TokenLimit.CollectionChanged += Tokens_CollectionChanged;
        }

        internal static void TrimEnd(ref List<char> arr, int amount)
        {
            arr.RemoveRange(arr.Count - amount, amount);
        }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(string text)
        {
            Text = ToCharArray(text);
            PerformStart();
        }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(StringBuilder text)
        {
            text.CopyTo(0, Text, 0, text.Length);
            PerformStart();
        }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(params char[] text)
        {
            Text = text;

            // Start the code
            PerformStart();
        }

        /// <summary>
        /// Stops the execution of the parser.
        /// </summary>
        public void Stop()
        {
            // We aren't processing anymore.
            IsProcessing = false;

            // Make sure we definitely complete the queued up token.
            PerformToken(null);

            // Reset the queue back to null in case the parser gets ran again.
            _queue = null;

            // Reset the variables which decide which build ups to use back to their default values.
            _usePrimary = true;
            _togglePrimary = false;

            // Reset the build ups.
            _primaryBuildUp.Clear();
            _secondaryBuildUp.Clear();

            // Reset some other variables.
            _textBuildup.Clear();
            _foundExactToken = false;

            // Make sure the token start goes back to its unset value.
            PossibleTokenStart = -1;
        }

        private void Tokens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //sender = e.NewItems;
            ManageStartChars();
        }

        /// <summary>
        /// Actually does the start...
        /// </summary>
        private void PerformStart()
        {
            //var tmr = Stopwatch.StartNew();

            // If we are ALREADY in the process of parsing, don't bother.
            if (IsProcessing)
                return;

            // Initialize all the correct variables - and call the OnStart method by initilizing everything.
            Init();

            // Loop through every character - this is the very heart of this parser.
            for (_currentLocation = 0; _currentLocation < Text.Length; _currentLocation++)
            {
                // Actually process the character!
                ProcessChar(true);

                // Stop executing if we aren't processing anymore.
                if (!IsProcessing)
                    break;
            }

            // Make sure update anything that needs to be updated - based on the last character.
            ProcessBuiltUpTokens(false);

            // Reset everything and stop the parser - the parser has finished.
            Stop();

            // Call the "OnEnd" method
            OnEnd();
            //var ticks = tmr.ElapsedTicks;
        }

        public void Init()
        {
            // Call the "OnStart" method
            OnStart();

            // Make sure we are parsing now
            IsProcessing = true;

            // Reset the location to the beginning - to make sure we don't continue from where we were before.
            CurrentLocation = 0;
            _currentLocation = 0;

            // Set the built up tokens back to default - ALL of the tokens.
            _builtUp = Tokens.ToList();
        }

        /// <summary>
        /// When a token is found.
        /// </summary>
        /// <param name="token"></param>
        private void PerformToken(ABParserToken token)
        {
            // This has to be done before handling any queued up items - 
            // If this token is more than one character long - the leading/trailing will now have a bit of it in it, so we need to remove that.
            if (token != null)
                if (token.Token.Length > 1)
                    if (_usePrimary)
                        TrimEnd(ref _primaryBuildUp, token.Token.Length - 1);
                    else
                        TrimEnd(ref _secondaryBuildUp, token.Token.Length - 1);

            // If there is already a token queued up, process it.
            if (_queue != null)
            {
                // Toggle _usePrimary if needed - because it was toggled to let it build up the trailing.
                _usePrimary = !_usePrimary;

                // Make sure the "CurrentLocation" variable is actually the location of this token.
                if (!UseMoveForwardToChangePosition)
                    CurrentLocation = _queueTokenEnd;

                // Clear out the token limit if the trailing of the next token shouldn't be limited.
                if (!LimitAffectsNextTrailing)
                {
                    TokenLimit.Clear();
                    ManageStartChars();

                    // Reset the variable.
                    LimitAffectsNextTrailing = true;
                }

                // Call the overrideable method.
                OnTokenProcessed(new TokenProcessedEventArgs(_queue, token, _queueTokenStart, _queueTokenEnd,
                    (_usePrimary) ? _primaryBuildUp : _secondaryBuildUp, (_usePrimary) ? _secondaryBuildUp : _primaryBuildUp));

                // Toggle _usePrimary if it wasn't done when this token was actually found.
                if (!_togglePrimary)
                    _usePrimary = !_usePrimary;
                //// Set the _queue to null.
                //_queue = null;

                // In order to make sure the user recieves everything in a logical order (the OnCharacterProcessed for the trailing data gets run AFTER the token is processed), we will need to recount all the trailing characters now.
                if (NotifyCharacterProcessed && !UseMoveForwardToChangePosition)
                {
                    // The amount of characters we have to recount.
                    var amount = _currentLocation - CurrentLocation;

                    // Make sure the public CurrentLocation is at the end of the recounting token.
                    CurrentLocation = _currentLocation - CurrentLocation;

                    // Recount all the trailing characters.
                    for (int i = 0; i < amount; i++)
                    {
                        OnCharacterProcessed(Text[CurrentLocation]);
                        CurrentLocation++;
                    }
                }
            }

            // Don't bother queuing up the token if it's null.
            if (token == null)
                return;

            // If we should change "_usePrimary"
            _togglePrimary = (_togglePrimary) ? false : true;

            // Toggle _usePrimary, if needed.
            if (_togglePrimary)
                _usePrimary = (_usePrimary) ? false : true;

            _foundExactToken = false;

            // Set the queued up start/end
            _queueTokenStart = PossibleTokenStart;
            _queueTokenEnd = PossibleTokenEnd;

            // Call the overrideable event.
            BeforeTokenProcessed(token);

            // Reset the token start to its unset value.
            PossibleTokenStart = -1;

            // Clear out the textBuildup so that it 
            _textBuildup.Clear();

            // Store whether the _queue is null.
            var wasNull = (_queue == null);

            // Queue up this token.
            _queue = token;

            // Don't do anything else if the _queue was null.
            if (wasNull)
                return;

            // Clear whatever build up is the OLD leading.
            if (_usePrimary)
                _primaryBuildUp.Clear();
            else
                _secondaryBuildUp.Clear();
        }

        /// <summary>
        /// The heart of the parser - processes a character.
        /// </summary>
        /// <returns>Whether this method should be called twice on the next character</returns>
        public bool ProcessChar(bool processBuiltUpTokens)
        {
            // If we should ignore this character for whatever reason... don't bother with checking the tokens, and just add it to the build up.
            if ((EscapeTokens && _charIsEscaped) || (IgnoreWhitespace && char.IsWhiteSpace(Text[_currentLocation])))
            {
                // If this character was escaped... then it isn't anymore!
                _charIsEscaped = false;

                // If there is a queued up item - we'll want to make sure we look at that.
                if (_foundExactToken && processBuiltUpTokens)
                    ProcessBuiltUpTokens(false);

                // Add this character to the correct build up.
                if (_usePrimary)
                    _primaryBuildUp.Add(Text[_currentLocation]);
                else
                    _secondaryBuildUp.Add(Text[_currentLocation]);
            } else {

                // Whether or not we should return at the end.
                var shouldReturn = false;

                // If this character is the escape character...
                if (Text[_currentLocation] == EscapeCharacter)
                {
                    _charIsEscaped = true;
                    return false;
                }

                // We add all the tokens that still fit, then place that into the main "_builtUp" array.
                var newBuildupTokens = new List<ABParserToken>();

                // Add the current character to the built up item.
                _textBuildup.Add(Text[_currentLocation]);

                // Check if these characters can even fit under any of the tokens... This is a performance thing.
                if (_textBuildup.Count == 1 && _builtUp.Count > 0)
                {
                    // Clear out the old _builtUp variables.
                    _builtUp.Clear();

                    // Go through all the first characters, and add rules that fit it...
                    for (int i = 0; i < _firstTokensChars.Length; i++)

                        // Check if this token fits, if so, add it to the built up tokens...
                        if (_textBuildup[0] == _firstTokensChars[i])
                            _builtUp.Add(_validTokens[i]);
                }

                // If the above test found something, the rest can be executed.
                if (_builtUp.Count > 0)
                {
                    // Put the _textBuildup into an array, which is used to compare with the Text array.
                    //var asArray = _textBuildup.ToArray();

                    // Check which token the current build-up text fits under, keeping track of how many are left.
                    for (int i = 0; i < _builtUp.Count; i++)

                        if (_builtUp[i].Token.SequenceEqual(_textBuildup))
                        {
                            // If we've already found an exact token - which this one DOESN'T replace, we need to get that dealt with - IMMEDIATELY.
                            if (_foundExactToken && !StartsWith(_builtUp[i].Token, _exactToken.Token))
                                PerformToken(_exactToken);

                            // Say that we've found a token with the exact same text as the build-up so far.
                            _foundExactToken = true;
                            _exactToken = _builtUp[i];

                            // Set this as the start of the token - if it hasn't already been set.
                            if (PossibleTokenStart == -1)
                                PossibleTokenStart = _currentLocation;

                            // Set this as the end of this token.
                            PossibleTokenEnd = _currentLocation;

                            // Make it so we return at the end
                            shouldReturn = true;
                        }

                        // If the current builtUp one we are testing does actually start with our current built-up string.
                        else if (_textBuildup.Count == 1 || StartsWith(_builtUp[i].Token, _textBuildup))
                        {
                            newBuildupTokens.Add(_builtUp[i]);

                            // Just in case this turns out to be the one we are looking for - store this as the start location, if it hasn't already been set.
                            if (PossibleTokenStart == -1)
                                PossibleTokenStart = _currentLocation;
                        }

                    // The possible tokens this could be.
                    _builtUp = newBuildupTokens;
                }

                // If we found an exact match, we'll leave it to deal with that on the next character.
                if (shouldReturn)
                    return true;

                // Check if the result of the for loop above gave back any tokens.
                if (processBuiltUpTokens)
                    ProcessBuiltUpTokens(true);
            }

            return false;
        }

        public void ProcessBuiltUpTokens(bool retry)
        {
            // If the text array is empty, don't bother.
            if (Text.Length == 0)
                return;

            // If we didn't find anything that it's LIKE, however, we did find an EXACT one... or if this is the last character, go ahead and perform it, and do some other things.
            if (_builtUp.Count == 0 || !retry)
                if (_foundExactToken)
                {
                    PerformToken(_exactToken);

                    // Reset the _builtUp array, since we're done with this token.
                    _builtUp = Tokens.ToList();

                    // Leave it to the next character to deal with this (just in case there's more to the token) - however, if there isn't a "next" character, check it now by processing this character again!
                    if (!retry || ProcessChar(false))
                        return;

                    //// Clear all the built up text AGAIN - because we just ran ProcessChar...
                    //_textBuildup.Clear();

                    //// Reset the _builtUp array, again - since we called the ProcessChar method.
                    //_builtUp = Tokens.ToList();

                    // If we HAVE found the exact token... Don't bother with the OnCharacterProcessed function - because this character will get recounted anyway.
                    //if (_foundExactToken)
                    //    return;
                }

            if (_builtUp.Count == 0)
            {
                // Whether we should reset the possible token start back to unset - this will happen only if there is no way this could be a token.
                bool unsetTokenStart = true;

                // Reset the _builtUp array, since the data in it right now means nothing.
                _builtUp = Tokens.ToList();

                // If there's more than one character in the Text Buildup - this character on its own could be a token,
                // in which case, we need to check just this character, and, if it is a token or part of one, don't clear out the Text BuildUp.
                if (_textBuildup.Count > 1)
                {
                    // Clear out the Text BuildUp, since, when we test this character on its own, it's going to automatically add this character back in.
                    _textBuildup.Clear();

                    // Test this character, if the method returned true, that means we've found an exact token - however, we also have to have found nothing that can POSSIBLY be a token (the BuildUp)
                    if (!ProcessChar(false) && _builtUp.Count == 0)
                        _textBuildup.Clear();

                    // If we haven't found an exact token, AND there's nothing LIKE this, reset our possible token BuildUp.
                    if (!_foundExactToken && _builtUp.Count == 0)
                        _builtUp = Tokens.ToList();

                    // Otherwise, if there's still a chance of it being a token - DON'T reset the PossibleTokenStart.
                    else unsetTokenStart = false;
                }

                // However, if there isn't more than one character built up... or we're just at the last character, just clear it out anyway.
                else _textBuildup.Clear();

                // If we should reset the possible token start, do it.
                if (unsetTokenStart) PossibleTokenStart = -1;
            }

            // Add to the current build up for leading/trailing.
            if (_currentLocation < Text.Length)
                if (_usePrimary)
                    _primaryBuildUp.Add(Text[_currentLocation]);
                else
                    _secondaryBuildUp.Add(Text[_currentLocation]);

            // If we should notify the user about when a character is changed, notify them... Unless we're building up the trailing
            if (NotifyCharacterProcessed && _queue == null)
                OnCharacterProcessed(Text[_currentLocation]);

            
        }

        /// <summary>
        /// Goes to the next character. NOTE: FOR NORMAL BEHAVIOUR ENABLE <see cref="UseMoveForwardToChangePosition"/>
        /// </summary>
        public void MoveForward()
        {
            // Move the actual location forward.
            _currentLocation++;

            // Since the user is manually changing positions, we'll change the public one as well.
            CurrentLocation = _currentLocation;
        }

        #endregion

        #region Overrideable Methods

        /// <summary>
        /// When the parser starts.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// When the parser finishes/ends.
        /// </summary>
        protected virtual void OnEnd() { }

        /// <summary>
        /// Before a token is COMPLETELY processed (before the trailing is made)
        /// </summary>
        /// <param name="token"></param>
        protected virtual void BeforeTokenProcessed(ABParserToken token) { }

        /// <summary>
        /// When a character is processed. NOTE: <see cref="NotifyCharacterProcessed"/> MUST BE TRUE.
        /// </summary>
        /// <param name="ch">The character being processed.</param>
        protected virtual void OnCharacterProcessed(char ch)
        {
            // Increase the public position, if the user isn't manually going through.
            if (!UseMoveForwardToChangePosition)
                CurrentLocation++;

            // Trigger the event.
            // TODO: Event Support.
        }

        /// <summary>
        /// When a token is processed.
        /// </summary>
        /// <param name="e">Info about the token being processed, and where it is relative to the text.</param>
        protected virtual void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            // This method is literally designed to be overrided... So, we only need to trigger the event.

            // Trigger the event.
            // TODO: More event support.
        }

        #endregion
    }
}
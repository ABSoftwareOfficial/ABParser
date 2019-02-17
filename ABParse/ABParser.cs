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
    public unsafe class ABParser
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
        /// The start location in memory of the text buildup.
        /// </summary>
        private char* _textBuildupLocStart;

        /// <summary>
        /// The current location we're at in the "_textBuildup"
        /// </summary>
        private char* _textBuildupLoc;

        /// <summary>
        /// The text so far that could make up a token.
        /// </summary>
        private char[] _textBuildup = new char[0];

        /// <summary>
        /// Can either be leading/trailing - primary and secondary have their roles swapped to save processing power copying memory over.
        /// </summary>
        private char[] _primaryBuildUp = new char[0];

        /// <summary>
        /// The pointer to the start of the primary build up.
        /// </summary>
        private char* _primaryBuildUpStart;

        /// <summary>
        /// The pointer to the current location in the primary build up.
        /// </summary>
        private char* _primaryBuildUpLocation;

        /// <summary>
        /// Can either be leading/trailing - primary and secondary have their roles swapped to save processing power copying memory over.
        /// </summary>
        private char[] _secondaryBuildUp = new char[0];

        /// <summary>
        /// The pointer to the start of in the secondary build up.
        /// </summary>
        private char* _secondaryBuildUpStart;

        /// <summary>
        /// The pointer to the current location in the secondary build up.
        /// </summary>
        private char* _secondaryBuildUpLocation;

        /// <summary>
        /// The amount of the current build up that is used.
        /// </summary>
        private int _currentBuildUpLength;

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
        private ABParserToken[] _builtUp;

        /// <summary>
        /// The amount of places at the start of the _builtUp that actually contain something.
        /// </summary>
        private int _builtUpLength;

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
        /// The possible start location of the next token, will be -1 when one hasn't been decided.
        /// </summary>
        public int PossibleTokenStart
        {
            get => _possibleTokenStart;
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

        #region Unsafe Array Management (StartsWith, TrimEnd etc.)
        //public unsafe static char[] ToCharArray(string str)
        //{
        //    // Create an array of characters.
        //    var chArray = new char[str.Length];

        //    // Fix a pointer to this new array and the string.
        //    fixed (char* fixedCharPointer = chArray, fixedStrPointer = str)
        //    {
        //        // Create non-fixed pointers for both of them.
        //        var charPointer = fixedCharPointer;
        //        var strPointer = fixedStrPointer;

        //        // Add the characters from the string to the char array!
        //        for (int i = 0; i < chArray.Length; i++)
        //            *(charPointer++) = *(strPointer++);
        //    }

        //    // Return the array.
        //    return chArray;
        //}

        /// <summary>
        /// Works like SequenceEqual, except it only checks up to the end of the "contents", and it can't just start with it.
        /// </summary>
        internal unsafe static bool EqualsOrStartsWithUpToNullCharacter(char* arr, long arrLength, char[] contents, out bool startsWith)
        {
            startsWith = true;
            fixed (char* fixedPointer2 = contents)
            {
                var pointer2 = fixedPointer2;

                // Go through and check if the characters are the same, stop if we reach the end of either one.
                for (int i = 0; i < contents.Length; i++)

                    if (arrLength == i)
                        return false;
                
                    // If this character didn't match, it can't have started with the "contents".
                    else if (*arr++ != *pointer2++)
                    {
                        startsWith = false;
                        return false;
                    }
            }

            // If we get here, that means there were no problem, so it was a perfect match.
            return true;
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

        /// <summary>
        /// For performance, the first character of every token is stored, alongside its token, this means that something can instantly be detected as a possible token or not.
        /// This method will create that array with the first character.
        /// </summary>
        internal unsafe void ManageStartChars()
        {
            var chArray = new char[Tokens.Count];
            var tkArray = new ABParserToken[Tokens.Count];

            fixed (char* resultPointer = chArray)
            {
                // Create a non-fixed pointer for the result.
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

        /// <summary>
        /// Copies across an array, based on two pointers. Used instead of <see cref="Marshal.Copy(IntPtr, char[], int, int)"/> due to lack of support for pointers (it uses IntPtr).
        /// </summary>
        internal unsafe void CopyArrayWithPointers(char* source, char* destination, long length)
        {
            for (long i = 0; i < length; i++)
                *destination++ = *source++;
        }

        /// <summary>
        /// Trims the current build up right down to what is actually the leading/trailing.
        /// </summary>
        internal unsafe void ShortenBuildUp(bool isLast)
        {
            char* correctBuildUp;
            long currentBuildUpFilled;

            // Figure out how much of the current build up has been filled.
            if (_usePrimary)
            {
                correctBuildUp = _primaryBuildUpStart;
                currentBuildUpFilled = _primaryBuildUpLocation - correctBuildUp;
            }

            // Do for both the primary and secondary build ups.
            else
            {
                correctBuildUp = _secondaryBuildUpStart;
                currentBuildUpFilled = _secondaryBuildUpLocation - correctBuildUp;
            }

            // Work out the amount of the build up that is actually the leading/trailing.
            long currentTokenLength = isLast ? 0 : _exactToken.Token.Length - 1;
            long correctLength = _currentBuildUpLength - currentTokenLength;

            // Create a new array to store the items while we reset the build up.
            var newArr = new char[correctLength];

            fixed (char* newArrFixed = newArr)
            {
                CopyArrayWithPointers(correctBuildUp, newArrFixed, correctLength);

                // Next, reset the build up array, with the correct size.
                if (_usePrimary)
                    ResetPrimaryBuildUp(correctLength);
                else
                    ResetSecondaryBuildUp(correctLength);

                // Finally, copy the items back.
                CopyArrayWithPointers(newArrFixed, _usePrimary ? _primaryBuildUpStart : _secondaryBuildUpStart, correctLength);
            }
        }

        #endregion

        #region Basic Interaction
        public ABParser()
        {
            Tokens.CollectionChanged += Tokens_CollectionChanged;
            TokenLimit.CollectionChanged += Tokens_CollectionChanged;
        }

        /// <summary>
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(string text)
        {
            Text = text.ToCharArray();
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
        /// Starts the parser.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void Start(StringBuilder text)
        {
            text.CopyTo(0, Text, 0, text.Length);
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
            HandleQueuedToken(null, true);

            // Reset the queue back to null in case the parser gets ran again.
            _queue = null;

            // Reset the variables which decide which build ups to use back to their default values.
            _usePrimary = true;
            _togglePrimary = false;

            // Make sure the token start goes back to its unset value.
            _foundExactToken = false;
            PossibleTokenStart = -1;
        }

        #endregion

        #region Extra

        public void Init()
        {
            // Call the "OnStart" method
            OnStart();

            // Make sure we are parsing now, and that we are
            IsProcessing = true;

            // Also, we don't want to toggle primary, by default, but we do want to use primary.
            _togglePrimary = false;
            _usePrimary = true;

            // Reset the location to the beginning - to make sure we don't continue from where we were before.
            CurrentLocation = 0;
            _currentLocation = 0;

            // Set the built up tokens back to default, which is all of the tokens.
            _builtUp = new ABParserToken[Tokens.Count];
            Tokens.CopyTo(_builtUp, 0);

            // Set the _builtUpLength as well, and clear out any limits that were set before.
            _builtUpLength = _builtUp.Length;
            TokenLimit = new ObservableCollection<char[]>();

            // Make sure the queue start locations are reset as well (since starting the parser will leave those at "0")
            _queueTokenStart = 0;
            _queueTokenEnd = 0;

            // Initialize the two primary/secondary build ups.
            ResetPrimaryBuildUp(Text.Length);
            ResetSecondaryBuildUp(Text.Length);

            // Also reset the text buildup.
            ResetTextBuildUp();
            
        }

        /// <summary>
        /// Goes to the next character. NOTE: IN ORDER TO USE THIS ENABLE <see cref="UseMoveForwardToChangePosition"/>
        /// </summary>
        public void MoveForward()
        {
            // Move the actual location forward.
            _currentLocation++;

            // Since the user is manually changing positions, we'll change the public one as well.
            CurrentLocation = _currentLocation;
        }

        private void Tokens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //sender = e.NewItems;
            ManageStartChars();
        }

        /// <summary>
        /// Resets the primary build up to a specified length.
        /// </summary>
        /// <returns>The new start location.</returns>
        private unsafe void ResetPrimaryBuildUp(long newSize)
        {
            // First, reset the length.
            _currentBuildUpLength = 0;

            // Then, recreate the array and make sure we go back to pointing to the start.
            _primaryBuildUp = new char[newSize];
            fixed (char* primaryBuildUpFixed = _primaryBuildUp)
                _primaryBuildUpLocation = _primaryBuildUpStart = primaryBuildUpFixed;
        }

        /// <summary>
        /// Resets the secondary build up to a specified length.
        /// </summary>
        /// <returns>The new start location.</returns>
        private unsafe void ResetSecondaryBuildUp(long newSize)
        {
            // First, reset the length.
            _currentBuildUpLength = 0;

            // Recreate the array and make sure we go back to pointing to the start.
            _secondaryBuildUp = new char[newSize];
            fixed (char* secondaryBuildUpFixed = _secondaryBuildUp)
                _secondaryBuildUpLocation = _secondaryBuildUpStart = secondaryBuildUpFixed;
        }

        private void ResetTextBuildUp()
        {
            // Recreate the array and make sure our start is correct.
            _textBuildup = new char[Text.Length - _currentLocation];
            fixed (char* textBuildUpFixed = _textBuildup)
                _textBuildupLocStart = textBuildUpFixed;

            // Make sure we move back to the start as well.
            _textBuildupLoc = _textBuildupLocStart;
        }

        #endregion

        #region Main Code

        /// <summary>
        /// Actually starts ABParser.
        /// </summary>
        private unsafe void PerformStart()
        {
            // If we are ALREADY in the process of parsing, don't start it again.
            if (IsProcessing)
                return;

            // Initialize all the correct variables - which will call the OnStart method by initilizing everything.
            Init();
            
            fixed (char* fixedCharPointer = Text, primaryCharPointer = _primaryBuildUp, secondaryCharPointer = _secondaryBuildUp, fixedTextBuildUp = _textBuildup)
            {
                var charPointer = fixedCharPointer;
                _textBuildupLoc = fixedTextBuildUp;

                // Set the start for the text buildup.
                _textBuildupLocStart = fixedTextBuildUp;

                // Make sure that we're at the start of the two build ups.
                _primaryBuildUpLocation = primaryCharPointer;
                _secondaryBuildUpLocation = secondaryCharPointer;

                // Loop through every character - this is the very heart of the parser.
                for (_currentLocation = 0; _currentLocation < Text.Length; _currentLocation++)
                {
                    // Actually process the character!
                    ProcessChar(*charPointer++, (_usePrimary) ? _primaryBuildUpLocation++ : _secondaryBuildUpLocation++, true);

                    // Stop executing if we aren't processing anymore.
                    if (!IsProcessing)
                        break;
                }

                // Make sure we update anything that needs to be updated.
                _currentLocation--;
                ProcessPossibleTokens(true, *(fixedCharPointer + Text.Length), (_usePrimary) ? primaryCharPointer : secondaryCharPointer);
            }

            // Reset everything and stop the parser - the parser has finished.
            Stop();

            // Call the "OnEnd" method
            OnEnd();
            //var ticks = tmr.ElapsedTicks;
        }

        /// <summary>
        /// The heart of the parser - processes a character.
        /// </summary>
        /// <returns>Whether the current text buildup on its own perfectly matched a token.</returns>
        public unsafe bool ProcessChar(char character, char* currentBuildUp, bool processPossibleTokens)
        {
            // If we should ignore this character for whatever reason... don't bother with checking the tokens, and just add it to the build up, and handle any exact tokens.
            if (_charIsEscaped || (IgnoreWhitespace && char.IsWhiteSpace(character)))
            {

                // If there is a queued up item - make sure that gets handled.
                if (_foundExactToken && processPossibleTokens)
                    ProcessPossibleTokens(true, character, currentBuildUp);

                // For an escaped character, add this character to the correct build up, but, do that while pushing the build up back, if this didn't happen, the escape character would just be left as an empty space in the build up.
                if (_charIsEscaped)
                    if (_usePrimary)
                        *(--_primaryBuildUpLocation - 1) = character;
                    else
                        *(--_secondaryBuildUpLocation - 1) = character;

                // Otherwise, if the character was not escaped, it can just put it in there normally.
                else
                    if (_usePrimary)
                        *(_primaryBuildUpLocation - 1) = character;
                    else
                        *(_secondaryBuildUpLocation - 1) = character;

                // Move the position in the current buildup forward.
                _currentBuildUpLength++;

                // This character is no longer escaped, if it was.
                _charIsEscaped = false;

            } else {


                // If this character is the escape character, then the next character will be ignored.
                if (EscapeTokens && character == EscapeCharacter)
                {
                    _charIsEscaped = true;
                    return false;
                }

                // Add the current character to the built up item.
                *_textBuildupLoc++ = character;

                // If we've only come across the first character of the token so far, we'll check the "_firstTokensChars" for performance.
                if (_textBuildupLoc - _textBuildupLocStart == 1 && _builtUpLength > 0)
                {
                    // Clear out the old _builtUp variables.
                    _builtUp = new ABParserToken[_firstTokensChars.Length];
                    _builtUpLength = 0;

                    // Go through all the first characters, and add rules that fit it...
                    fixed (char* fixedFirstTokensPointer = _firstTokensChars)
                    {
                        var firstTokenPointer = fixedFirstTokensPointer;

                        for (int i = 0; i < _firstTokensChars.Length; i++)

                            // Check if this token fits, if so, add it to the built up tokens.
                            if (*_textBuildupLocStart == *firstTokenPointer++)
                                _builtUp[_builtUpLength++] = _validTokens[i];
                    }
                }

                // If the above test found something, the rest can be executed.
                if (_builtUpLength > 0)
                {
                    // Whether or not we should return at the end - leaving everything to the next character.
                    var handleNextCharacter = false;

                    // We will now go through all the remaining tokens that are possible with the current text buildup, and attempt to narrow it down even more with this character.
                    var newBuildupTokens = new List<ABParserToken>();

                    // Check which token the current build-up text fits under, keeping track of how many are left.
                    for (int i = 0; i < _builtUpLength; i++)

                        if (EqualsOrStartsWithUpToNullCharacter(_textBuildupLocStart, _textBuildupLoc - _textBuildupLocStart, _builtUp[i].Token, out bool startsWith))
                        {
                            // If we've already found an exact token - which this one DOESN'T replace (this one replaces it if it starts the same), we need to get that one handled first.
                            if (_foundExactToken && !StartsWith(_builtUp[i].Token, _exactToken.Token))
                                PerformToken(_exactToken);

                            // We've now found a token with the exact same text as the build-up so far (there could be some other tokens that start with this though).
                            _foundExactToken = true;
                            _exactToken = _builtUp[i];

                            // Set this as the start of the token - if it hasn't already been set.
                            if (PossibleTokenStart == -1)
                                PossibleTokenStart = _currentLocation;

                            // Set this as the end of this token, since, because we've just come across this token, the end would clearly be where we are.
                            PossibleTokenEnd = _currentLocation;

                            // Leave it to the next character to handle this, just in case it's actually a longer token which just so happens to contain the Text BuildUp.
                            handleNextCharacter = true;
                        }

                        // If the current builtUp one we are testing does actually START with our current built-up string.
                        else if (_textBuildupLocStart - _textBuildupLoc == 1 || startsWith)
                        {
                            newBuildupTokens.Add(_builtUp[i]);

                            // Just in case this turns out to be the one we are looking for - store this as the start location, if it hasn't already been set.
                            if (PossibleTokenStart == -1)
                                PossibleTokenStart = _currentLocation;
                        }

                    // The possible tokens this could be.
                    newBuildupTokens.CopyTo(_builtUp);
                    _builtUpLength = newBuildupTokens.Count;

                    // If we found an exact match, we'll leave it to deal with that on the next character, in case it's actually part of a bigger token.
                    if (handleNextCharacter)
                        return true;
                }

                // Check if the result of the for loop above gave back any tokens.
                if (processPossibleTokens)
                    ProcessPossibleTokens(false, character, currentBuildUp);
            }

            return false;
        }

        /// <summary>
        /// After a character is processed, this will look at what possible tokens the build up could be.
        /// </summary>
        /// <returns>The new buildup.</returns>
        public unsafe void ProcessPossibleTokens(bool whitespaceOrLastCharacter, char character, char* currentBuildUp)
        {

            // If we didn't find anything that it's LIKE, however, we did find an EXACT one...
            // However, if we're on the last character, or whitespace, there won't be anything else after (that could be because it's whitespace), so if we've found an exact token, go ahead and handle.
            if (_builtUpLength == 0 || whitespaceOrLastCharacter)
                if (_foundExactToken)
                {
                    PerformToken(_exactToken);
                    currentBuildUp = _usePrimary ? _primaryBuildUpLocation++ : _secondaryBuildUpLocation++;

                    // Reset the _builtUp array, since the data in it right now means nothing.
                    _builtUpLength = Tokens.Count;
                    _builtUp = new ABParserToken[_builtUpLength];

                    // Copy all these across.
                    Tokens.CopyTo(_builtUp, 0);

                    // Check if this character on its own wasn't a token, if it was, return so the next character can handle it, just in case it's ALSO part of another bigger token.
                    // Don't do that check at all if this is the last character, though.
                    if (whitespaceOrLastCharacter || ProcessChar(character, currentBuildUp, false))
                        return;
                }

            if (_builtUpLength == 0)
            {
                // Whether we should reset the possible token start back to unset - this will happen only if there is no way this could be a token.
                bool unsetTokenStart = true;
               
                // Reset the _builtUp array, since the data in it right now means nothing.
                _builtUpLength = Tokens.Count;
                _builtUp = new ABParserToken[_builtUpLength];

                // Copy all these across.
                Tokens.CopyTo(_builtUp, 0);

                // If there's more than one character in the Text Buildup - this character on its own could be a token,
                // in which case, we need to check just this character, and, if it is a token or part of one, don't clear out the Text BuildUp.
                if (_textBuildupLoc - _textBuildupLocStart > 1)
                {
                    // Clear out the Text BuildUp, since, when we test this character on its own, it's going to automatically add this character back in.
                    ResetTextBuildUp();

                    // Test this character, if the method returned true, that means we've found an exact token - however, we also have to have found nothing that can POSSIBLY be a token (the BuildUp)
                    if (!ProcessChar(character, currentBuildUp, false) && _builtUp.Length == 0)
                        ResetTextBuildUp();

                    // If we haven't found an exact token, AND there's nothing LIKE this, reset our possible token BuildUp.
                    if (!_foundExactToken && _builtUp.Length == 0)
                        Tokens.CopyTo(_builtUp, 0);

                    // Otherwise, if there's still a chance of it being a token - DON'T reset the PossibleTokenStart.
                    else unsetTokenStart = false;
                }

                // If there isn't more than one character built up... no need to completely reset the text buildup, just keep the pointer at the beginning and it will get overwritten.
                else _textBuildupLoc = _textBuildupLocStart;

                // If we should reset the possible token start, do it.
                if (unsetTokenStart) PossibleTokenStart = -1;
            }

            // Add to the current build up for leading/trailing.
            if (!whitespaceOrLastCharacter)
            {
                *currentBuildUp = character;
                _currentBuildUpLength++;
            }
                
            // If we should notify the user about when a character is changed, notify them... Unless we're building up the trailing, in which case we will "recount" them later.
            if (NotifyCharacterProcessed && _queue == null)
                OnCharacterProcessed(character);

            
        }

        /// <summary>
        /// When a token is found, this will handle executing "BeforeTokenProcessed" and "OnTokenProcessed", as well as getting the leading/trailing all ready.
        /// </summary>
        private void PerformToken(ABParserToken token)
        {
            // Process the token that was queued up.
            HandleQueuedToken(token, false);

            // Don't bother queuing up the token if there isn't one to queue up (the token is usually null to make sure that the queued up token happens)
            if (token == null)
                return;

            // Decide if we should toggle "usePrimary".
            _togglePrimary = !_togglePrimary;

            // Toggle _usePrimary, if needed.
            if (_togglePrimary)
                _usePrimary = !_usePrimary;

            _foundExactToken = false;

            // Set the queued up start/end
            _queueTokenStart = PossibleTokenStart;
            _queueTokenEnd = PossibleTokenEnd;

            // Call the overrideable event.
            BeforeTokenProcessed(token);

            // Reset the token start to its unset value.
            PossibleTokenStart = -1;

            // Clear out the textBuildup so that it 
            ResetTextBuildUp();

            // Store whether the _queue is null, so that we can check that in just a moment, then queue up this token.
            var wasNull = (_queue == null);
            _queue = token;

            // Don't clear up the old build up if the queue was null, because that means we're on the first token, and there's no leading/trailing to clear on it.
            if (wasNull)
                return;

            // Clear whatever build up is the OLD leading.
            if (_usePrimary)
                ResetPrimaryBuildUp(Text.Length - _currentLocation);
            else
                ResetSecondaryBuildUp(Text.Length - _currentLocation);
        }

        private void HandleQueuedToken(ABParserToken nextToken, bool isLast)
        {
            // This has to be done before handling any queued up items - 
            // If this token is more than one character long - the leading/trailing will now have a bit of it in it, so we need to remove that.
            // Not only that, but, this method will also remove any trailing "\0"s, hence why we call it even if the token isn't more than one character long.
            ShortenBuildUp(isLast);

            // If there is already a token queued up, process it.
            if (_queue != null)
            {
                // Toggle _usePrimary - because it was toggled to let it build up the trailing.
                _usePrimary = !_usePrimary;

                // Make sure the "CurrentLocation" variable is actually the location of this token.
                if (!UseMoveForwardToChangePosition)
                    CurrentLocation = _queueTokenEnd;

                // Clear out the token limit if the trailing of the next token shouldn't be limited.
                if (!LimitAffectsNextTrailing)
                {
                    TokenLimit.Clear();
                    LimitAffectsNextTrailing = true;
                }

                // Call the overrideable method.
                OnTokenProcessed(new TokenProcessedEventArgs(_queue, nextToken, _queueTokenStart, _queueTokenEnd,
                    (_usePrimary) ? _primaryBuildUp : _secondaryBuildUp, (_usePrimary) ? _secondaryBuildUp : _primaryBuildUp));

                // Toggle _usePrimary if it wasn't done when this token was actually found.
                if (!_togglePrimary)
                    _usePrimary = !_usePrimary;

                // In order to make sure the user recieves everything in a logical order (the OnCharacterProcessed for the trailing data gets run AFTER the token is processed), we will need to recount all the trailing characters now.
                if (NotifyCharacterProcessed && !UseMoveForwardToChangePosition)
                {
                    // The amount of characters we have to recount.
                    var amount = _currentLocation - _queueTokenEnd;

                    // Make sure the public CurrentLocation is at the end of the recounting token.
                    CurrentLocation = amount-- + 1;

                    // Recount all the trailing characters.
                    for (int i = 0; i < amount; i++)
                    {
                        OnCharacterProcessed(Text[CurrentLocation]);
                        CurrentLocation++;
                    }
                }
            }
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
        /// This does not execute for tokens.
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
        protected virtual void OnTokenProcessed(TokenProcessedEventArgs e) { }

        #endregion
    }
}
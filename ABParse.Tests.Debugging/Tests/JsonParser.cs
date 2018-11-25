using ABParse.Tests.Debugging.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging.JSONVideo
{
    public enum ScriptJsonParserTokens
    {
        ObjectStart,
        ObjectEnd,
        ArrayStart,
        ArrayEnd,
        String,
        PairSeperator,
        ItemSeperator
    }

    /// <summary>
    /// USED WHILE WRITING THE SCRIPT FOR "Making Your Own JSON parser" TUTORIAL - A JSON parser to parse JSON.
    /// </summary>
    public class ScriptWritingJsonParser : ABParser
    {
        /// <summary>
        /// The final result of the parser.
        /// </summary>
        public JObject Result;

        /// <summary>
        /// The objects we're currently in.
        /// </summary>
        public List<IJValue> CurrentObjects;

        /// <summary>
        /// The locations that we're currently at in for each of the <see cref="CurrentObjects"/>.
        /// </summary>
        public List<int> CurrentObjectLocations;

        /// <summary>
        /// Whether we're on the value of a pair or not.
        /// </summary>
        public bool OnValue;

        /// <summary>
        /// Whether we're in a string or not.
        /// </summary>
        public bool InString;

        /// <summary>
        /// Script.
        /// </summary>
        public ScriptWritingJsonParser()
        {
            Tokens = new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
            {
                new ABParserToken(nameof(JsonParserTokens.ObjectStart), '{'),
                new ABParserToken(nameof(JsonParserTokens.ObjectEnd), '}'),
                new ABParserToken(nameof(JsonParserTokens.ArrayStart), '['),
                new ABParserToken(nameof(JsonParserTokens.ArrayEnd), ']'),
                new ABParserToken(nameof(JsonParserTokens.String), '"'),
                new ABParserToken(nameof(JsonParserTokens.PairSeperator), ':'),
                new ABParserToken(nameof(JsonParserTokens.ItemSeperator), ',')
            };
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Reset the values.
            CurrentObjects = new List<IJValue>();
            CurrentObjectLocations = new List<int>();
        }

        protected override void BeforeTokenProcessed(ABParserToken token)
        {
            base.BeforeTokenProcessed(token);

            // Set a TokenLimit for the string, if we've come across one.
            if (token.Name == nameof(JsonParserTokens.String) && !InString)
            {
                LimitAffectsNextTrailing = false;
                TokenLimit = new System.Collections.ObjectModel.ObservableCollection<char[]>() { token.Token };
            }
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            base.OnTokenProcessed(e);

            if (InString)
            {
                // If we're on a value, set the value as the correct string.
                if (OnValue)
                    SetValue(new JPrimitive(JPrimitiveType.String, e.Leading), false);
                
                // Otherwise, add a new pair with this key.
                else
                    (CurrentObjects[CurrentObjects.Count - 1] as JObject).Pairs.Add(new JPair(e.Leading));

                // Mark us as no longer being in a string anymore.
                InString = false;
            }
            else
                switch (e.Token.Name)
                {
                    case nameof(JsonParserTokens.ObjectStart):

                        // Add an item to the "CurrentObjects" and "CurrentObjectLocations".
                        CurrentObjects.Add(new JObject());
                        CurrentObjectLocations.Add(0);

                        // If we're going inside an object, it starts with a key, so, make sure "OnValue" is false.
                        OnValue = false;

                        break;
                    case nameof(JsonParserTokens.ObjectEnd):

                        // If this is the last item, place it into the final "Result".
                        if (CurrentObjects.Count == 1)
                            Result = (JObject)CurrentObjects.Last();

                        // Otherwise, finish with this item.
                        else
                            FinishItem(e.Leading);

                        break;
                    case nameof(JsonParserTokens.ArrayStart):

                        // Add an item to the "CurrentObjects" and "CurrentObjectLocations".
                        CurrentObjects.Add(new JArray());
                        CurrentObjectLocations.Add(0);

                        // Everything inside an object is technically a value, so make sure that "OnValue" is true.
                        OnValue = true;

                        // If there's actually something inside the array, add an item for it.
                        if (e.Trailing.Trim() != "" || e.NextToken.Name != nameof(JsonParserTokens.ArrayEnd))
                            (CurrentObjects.Last() as JArray).Items.Add(null);

                        break;
                    case nameof(JsonParserTokens.ArrayEnd):

                        // Finish up with this array.
                        FinishItem(e.Leading);
                        break;
                    case nameof(JsonParserTokens.PairSeperator):

                        // Mark us as now being on the value.
                        OnValue = true;
                        break;
                    case nameof(JsonParserTokens.ItemSeperator):

                        // Parse the leading value and place it into the correct place.
                        ParseValueAndPlace(e.Leading);

                        // Move forward one.
                        CurrentObjectLocations[CurrentObjectLocations.Count - 1]++;

                        // Add a new item if we're in an array.
                        if (CurrentObjects.Last() is JArray)
                            (CurrentObjects.Last() as JArray).Items.Add(null);

                        // Otherwise, we're no longer on the value since the next thing will be a key.
                        else
                            OnValue = false;

                        break;

                    case nameof(JsonParserTokens.String):

                        // Mark us as being in a string now.
                        InString = true;
                        break;

                }
        }

        /// <summary>
        /// Parses a value and places it into the correct place.
        /// </summary>
        /// <param name="e"></param>
        private void ParseValueAndPlace(string leading)
        {
            // If the leading is empty, we don't want to do anything, because that means the value has already been filled.
            if (leading.Trim() != "")
            {
                // Parse the value found before this item seperator.
                var value = ParseValue(leading);

                // Put the value in the correct place.
                SetValue(value, false);
            }
        }

        /// <summary>
        /// Finishes up with an item.
        /// </summary>
        private void FinishItem(string leading)
        {
            // The last value won't get parsed with a comma, so make sure we parse it here.
            ParseValueAndPlace(leading);

            // Place the current object into the where it's meant to go before removing it.
            SetValue(CurrentObjects.Last(), true);

            // Remove the item.
            CurrentObjects.RemoveAt(CurrentObjects.Count - 1);
        }

        /// <summary>
        /// Places a value into an item.
        /// </summary>
        private void SetValue(IJValue value, bool secondToLast)
        {
            // Decide how to place the item into the object and then place it at the correct location.
            if (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] is JObject)
                (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] as JObject).Pairs[CurrentObjectLocations[secondToLast ? CurrentObjectLocations.Count - 2 : CurrentObjectLocations.Count - 1]].Value = value;
            else
                (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] as JArray).Items[CurrentObjectLocations[secondToLast ? CurrentObjectLocations.Count - 2 : CurrentObjectLocations.Count - 1]] = value;
        }

        /// <summary>
        /// Parses a string an gets a value out of it - assuming the string is a primitive.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private IJValue ParseValue(string value)
        {
            // Trim both the start and the end, so that we don't include any whitespace.
            var trimmed = value.Trim().TrimEnd();

            // Booleans
            if (trimmed == "true")
                return new JPrimitive(JPrimitiveType.Boolean, true);
            else if (trimmed == "false")
                return new JPrimitive(JPrimitiveType.Boolean, false);

            // Null
            else if (trimmed == "null")
                return null;

            // Numbers
            else
                // REMEMBER: IN A REAL PARSER YOU WOULD USE TryParse()
                return new JPrimitive(JPrimitiveType.Numerical, double.Parse(trimmed));
        }
    }
}

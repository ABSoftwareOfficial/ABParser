﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging.Other
{
    public enum JsonParserTokens
    {
        ObjectStart,
        ObjectEnd,
        ArrayStart,
        ArrayEnd,
        PairSeperator,
        ItemSeperator,
        String
    }

    public class JsonParser : ABParser
    {

        /// <summary>
        /// The final result.
        /// </summary>
        public JObject Result;

        /// <summary>
        /// Contains all the objects we're currently in.
        /// </summary>
        public List<IJValue> CurrentObjects;

        /// <summary>
        /// Contains all the different locations we're at for each object.
        /// </summary>
        public List<int> CurrentObjectLocations;

        /// <summary>
        /// Used to decide whether on a value or not.
        /// </summary>
        public bool OnValue;

        /// <summary>
        /// Used to detect whether we're in a string or not.
        /// </summary>
        public bool InString;
        
        public JsonParser()
        {
            Tokens = new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
            {
                new ABParserToken(nameof(JsonParserTokens.ObjectStart), '{'),
                new ABParserToken(nameof(JsonParserTokens.ObjectEnd), '}'),
                new ABParserToken(nameof(JsonParserTokens.ArrayStart), '['),
                new ABParserToken(nameof(JsonParserTokens.ArrayEnd), ']'),
                new ABParserToken(nameof(JsonParserTokens.String), '"'),
                new ABParserToken(nameof(JsonParserTokens.PairSeperator), ':'),
                new ABParserToken(nameof(JsonParserTokens.ItemSeperator), ','),
            };
        }

        protected override void OnStart()
        {
            // Reset everything.
            CurrentObjects = new List<IJValue>();
            CurrentObjectLocations = new List<int>();
        }

        protected override void BeforeTokenProcessed(ABParserToken token)
        {
            // Only do something if we've come across the first string token for a string.
            if (token.Name == nameof(JsonParserTokens.String) && !InString)
            {
                LimitAffectsNextTrailing = false;
                TokenLimit = new System.Collections.ObjectModel.ObservableCollection<char[]> { token.Token };
            }
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            if (InString)
            {
                // Mark us no longer in a string.
                InString = false;

                // If this string in a value, we'll want to set the value.
                if (OnValue)
                    SetValue(new JPrimitive(JPrimitiveType.String, e.Leading), false);

                // If this string is in the name, we'll want a new pair.
                else
                    (CurrentObjects[CurrentObjects.Count - 1] as JObject).Pairs.Add(new JPair(e.Leading));
            }
            else
                switch (e.Token.Name)
                {
                    case nameof(JsonParserTokens.ObjectStart):

                        // Add a new JObject to the CurrentObjects.
                        CurrentObjects.Add(new JObject());
                        CurrentObjectLocations.Add(0);

                        // If we're starting an object, the first thing we come across will always be a key.
                        OnValue = false;

                        break;
                    case nameof(JsonParserTokens.ObjectEnd):

                        // Check if there's one item.
                        if (CurrentObjects.Count == 1)
                            Result = CurrentObjects.Last() as JObject;

                        // Finish with the item.
                        else
                            FinishItem(e.Leading);

                        break;

                    case nameof(JsonParserTokens.ArrayStart):

                        // Add a new JArray to the CurrentObjects.
                        CurrentObjects.Add(new JArray());
                        CurrentObjectLocations.Add(0);

                        // Everything in an array, is technically a value.
                        OnValue = true;

                        // Add an item if the array isn't empty.
                        if (e.Trailing.Trim() != "" || e.NextToken.Name != nameof(JsonParserTokens.ArrayEnd))
                            (CurrentObjects[CurrentObjects.Count - 1] as JArray).Items.Add(null);

                        break;

                    case nameof(JsonParserTokens.ArrayEnd):

                        // Finish with the item.
                        FinishItem(e.Leading);

                        break;

                    case nameof(JsonParserTokens.PairSeperator):

                        // Mark us as being on the value.
                        OnValue = true;

                        break;

                    case nameof(JsonParserTokens.ItemSeperator):

                        // Parse the possible value found before this seperator.
                        ParseValueAndPlace(e.Leading);

                        // Move forward an item.
                        CurrentObjectLocations[CurrentObjectLocations.Count - 1]++;

                        // When we're in an array, we need to add a new item.
                        if (CurrentObjects.Last() is JArray)
                            (CurrentObjects[CurrentObjects.Count - 1] as JArray).Items.Add(null);

                        // If we're in an object, mark us as longer being on the value.
                        else
                            OnValue = false;

                        break;
                    case nameof(JsonParserTokens.String):

                        // We're currently in a string.
                        InString = true;
                        break;
                }
        }

        /// <summary>
        /// Attempts to parse a value found before a token.
        /// </summary>
        /// <param name="e"></param>
        private void ParseValueAndPlace(string leading)
        {
            // Only place the value if it hasn't already been parsed.
            if (leading.Trim() != "")
            {

                // Parse the value.
                var value = ParseValue(leading);

                // Place the value.
                SetValue(value, false);
            }
        }

        /// <summary>
        /// Parses a primitive.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed result.</returns>
        private IJValue ParseValue(string value)
        {
            // Get rid of all whitespace.
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
                return new JPrimitive(JPrimitiveType.Numerical, double.Parse(trimmed));
        }

        /// <summary>
        /// Finishes with an item.
        /// </summary>
        private void FinishItem(string leading)
        {
            // Parse the value before finishing with the item.
            ParseValueAndPlace(leading);

            // Place the item into the previous pair.
            SetValue(CurrentObjects.Last(), true);

            // Remove the item.
            CurrentObjects.RemoveAt(CurrentObjects.Count - 1);
            CurrentObjectLocations.RemoveAt(CurrentObjectLocations.Count - 1);
        }

        /// <summary>
        /// Sets a value at either the last or second-to-last item in the "CurrentObjects".
        /// </summary>
        /// <param name="value">The value to set it to.</param>
        /// <param name="secondToLast">Whether it's the second-to-last item.</param>
        private void SetValue(IJValue value, bool secondToLast)
        {
            if (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] is JObject)
                (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] as JObject).Pairs[CurrentObjectLocations[secondToLast ? CurrentObjectLocations.Count - 2 : CurrentObjectLocations.Count - 1]].Value = value;
            else
                (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] as JArray).Items[CurrentObjectLocations[secondToLast ? CurrentObjectLocations.Count - 2 : CurrentObjectLocations.Count - 1]] = value;
        }
    }
}

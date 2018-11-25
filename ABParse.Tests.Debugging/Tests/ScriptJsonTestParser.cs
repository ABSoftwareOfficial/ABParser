using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ABParse.Tests.Debugging.Other;

namespace ABParse.Tests.Debugging.JSON_Test
{
    /// <summary>
    /// A JSON parser created with ABParser!
    /// </summary>
    public class JsonTestParser : ABParser
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

        public JsonTestParser()
        {
            //Tokens = new ObservableCollection<ABParserToken>()
            //{
            //    new ABParserToken(nameof(JsonParserTokens.ObjectStart), '{'),
            //    new ABParserToken(nameof(JsonParserTokens.ObjectEnd), '}'),
            //    new ABParserToken(nameof(JsonParserTokens.ArrayStart), '['),
            //    new ABParserToken(nameof(JsonParserTokens.ArrayEnd), ']'),
            //    new ABParserToken(nameof(JsonParserTokens.String), '"'),
            //    new ABParserToken(nameof(JsonParserTokens.PairSeperator), ':'),
            //    new ABParserToken(nameof(JsonParserTokens.ItemSeperator), ',')
            //};
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Reset everything.
            CurrentObjects = new List<IJValue>();
            CurrentObjectLocations = new List<int>();
        }

        protected override void BeforeTokenProcessed(ABParserToken token)
        {
            base.BeforeTokenProcessed(token);

            // If we've hit a string token, set the TokenLimit.
            if (token.Name == nameof(JsonParserTokens.String) && !InString)
            {
                LimitAffectsNextTrailing = false;
                TokenLimit = new ObservableCollection<char[]>() { token.Token };
            }
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            base.OnTokenProcessed(e);

            // If we were in a string, we've now come across the second string token.
            if (InString)
            {
                // Add the contents string to whereever it needs to go (key, value).
                if (OnValue)
                    SetValue(new JPrimitive(JPrimitiveType.String, e.Leading), false);
                else
                    (CurrentObjects[CurrentObjects.Count - 1] as JObject).Pairs.Add(new JPair(e.Leading));

                // Mark us as no longer being in a string.
                InString = false;
            }
            else
                switch (e.Token.Name)
                {
                    case nameof(JsonParserTokens.ObjectStart):

                        // Add the object to the "CurrentObjects" and add an item to the "CurrentObjectLocations".
                        CurrentObjects.Add(new JObject());
                        CurrentObjectLocations.Add(0);

                        // The first thing in an object will be a key, so set "OnValue" to false.
                        OnValue = false;

                        break;

                    case nameof(JsonParserTokens.ObjectEnd):

                        // If this is the LAST object, there's no where else to for it to go, so put it into the Result.
                        if (CurrentObjects.Count == 1)
                            Result = (JObject)CurrentObjects.Last();

                        // Otherwise, Finish up with the object and put it in the previous object/array.
                        else
                            FinishItem(e);

                        break;
                    case nameof(JsonParserTokens.ArrayStart):

                        // Add the array to the "CurrentObjects" and add an item to the "CurrentObjectLocations".
                        CurrentObjects.Add(new JArray());
                        CurrentObjectLocations.Add(0);

                        // Inside an array, everything is an array, so make sure that "OnValue" is true.
                        OnValue = true;

                        // Add a new item, as long as there's actually something in there.
                        if (e.Trailing.Trim() != "" || e.NextToken.Name != nameof(JsonParserTokens.ArrayEnd))
                            (CurrentObjects.Last() as JArray).Items.Add(null);

                        break;
                    case nameof(JsonParserTokens.ArrayEnd):

                        // Finish up with the array.
                        FinishItem(e);

                        break;
                    case nameof(JsonParserTokens.PairSeperator):

                        // Move on to the value.
                        OnValue = true;

                        break;
                    case nameof(JsonParserTokens.ItemSeperator):

                        // Parse the item we just on.
                        ParsePreviousItem(e);

                        // Move forward one.
                        CurrentObjectLocations[CurrentObjectLocations.Count - 1]++;

                        // If we're in an array, add a new item.
                        if (CurrentObjects.Last() is JArray)
                            (CurrentObjects.Last() as JArray).Items.Add(null);

                        // As long as we're not in an array, we're no longer on the value.
                        else
                            OnValue = false;

                        break;

                    case nameof(JsonParserTokens.String):

                        // Mark us a being in a string.
                        InString = true;

                        break;
                }
        }

        private void ParsePreviousItem(TokenProcessedEventArgs e)
        {
            // If the leading is empty, it means we've already parsed the value (meaning that it was either: an object, an array OR a string)
            if (e.Leading.Trim() != "")
            {
                // Parse the value in the leading.
                var value = ParseValue(e.Leading);

                // Place the value in the correct place.
                SetValue(value, false);
            }
        }

        /// <summary>
        /// Finishes an item in the <see cref="CurrentObjects"/> array, by putting where it needs to go.
        /// </summary>
        private void FinishItem(TokenProcessedEventArgs e)
        {
            // Parse the previous item we were on.
            ParsePreviousItem(e);

            // Set the value to where it needs to go, which is the second-to-last CurrentObject.
            SetValue(CurrentObjects.Last(), true);

            // Now, remove this item from the "CurrentObjects", as well as the "CurrentObjectLocations".
            CurrentObjects.RemoveAt(CurrentObjects.Count - 1);
            CurrentObjectLocations.RemoveAt(CurrentObjectLocations.Count - 1);
        }

        /// <summary>
        /// Sets the current value of the last <see cref="CurrentObjects"/>.
        /// </summary>
        private void SetValue(IJValue value, bool secondToLast)
        {
            // Put the object where it needs to go, before removing it.
            if (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] is JObject)
                (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] as JObject).Pairs[CurrentObjectLocations[secondToLast ? CurrentObjectLocations.Count - 2 : CurrentObjectLocations.Count - 1]].Value = value;
            else
                (CurrentObjects[secondToLast ? CurrentObjects.Count - 2 : CurrentObjects.Count - 1] as JArray).Items[CurrentObjectLocations[secondToLast ? CurrentObjectLocations.Count - 2 : CurrentObjectLocations.Count - 1]] = value;
        }

        public IJValue ParseValue(string value)
        {
            // Booleans
            if (value.Trim().TrimEnd() == "false")
                return new JPrimitive(JPrimitiveType.Boolean, false);
            else if (value.Trim().TrimEnd() == "true")
                return new JPrimitive(JPrimitiveType.Boolean, true);

            // Null
            else if (value.Trim().TrimEnd() == "null")
                return null;

            // Numbers
            else
            {
                // NOTE: THIS IS ONLY A DEMO, SO, WE'RE NOT GOING TO BOTHER WITH TryParse().
                return new JPrimitive(JPrimitiveType.Numerical, double.Parse(value));
            }
        }
    }
}

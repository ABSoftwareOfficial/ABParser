using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging
{
    public struct JObject : IJValue
    {
        public JObject(List<JPair> pairs, string key) { Pairs = pairs; Key = key; }

        public List<JPair> Pairs;
        internal string Key;
    }

    public interface IJValue { }

    public struct JArray : IJValue
    {
        public JArray(List<IJValue> items, string key) { Items = items; Key = key; }

        public List<IJValue> Items;
        internal string Key;
    }

    public struct JPrimitive : IJValue
    {
        public object Data;
        public JPrimitiveType Type;

        public JPrimitive(JPrimitiveType type, object data)
        {
            Type = type;
            Data = data;
        }
    }

    public struct JPair
    {
        public string Key;
        public IJValue Value;
    }

    public enum JPrimitiveType
    {
        String,
        Boolean,
        Numerical
    }

    public enum JsonParserTokens
    {
        ObjectStart,
        ObjectEnd,
        ArrayStart,
        ArrayEnd,
        String,
        PairSeperator,
        ItemSeperator
    }

    public class JsonParser : ABParser
    {
        public JObject Result;
        public List<IJValue> Parents = new List<IJValue>();

        public JPair CurrentPair = new JPair();

        public bool OnValue;
        public bool InString;

        public JsonParser()
        {
            Tokens = new ObservableCollection<ABParserToken>()
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

        protected override void BeforeTokenProcessed(ABParserToken token)
        {
            base.BeforeTokenProcessed(token);

            if (token.Name == nameof(JsonParserTokens.String) && !InString)
            {
                // Make sure that the next string token won't have this rule on its trailing.
                LimitAffectsNextTrailing = false;

                // Set the token limit.
                TokenLimit = new ObservableCollection<char[]>() { token.Token };
            }
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            base.OnTokenProcessed(e);

            if (InString)
                InString = false;
            else {

                switch (e.Token.Name)
                {
                    case nameof(JsonParserTokens.ObjectStart):
                        Parents.Add(new JObject(new List<JPair>(), CurrentPair.Key));

                        // We don't want the pairs inside this object thinking they're values...
                        OnValue = false;
                        break;
                    case nameof(JsonParserTokens.ObjectEnd):
                        // If this is the parent at the very top... Set this as the final result.
                        if (Parents.Count == 1)
                            Result = (JObject)Parents[0];

                        // Add The Last Pair
                        ((JObject)Parents[Parents.Count - 1]).Pairs.Add(CurrentPair);

                        CurrentPair.Key = ((JObject)Parents[Parents.Count - 1]).Key;
                        CurrentPair.Value = Parents[Parents.Count - 1];
                        Parents.RemoveAt(Parents.Count - 1);

                        break;
                    case nameof(JsonParserTokens.ArrayStart):
                        // Everything inside an array is a value
                        OnValue = true;
                        Parents.Add(new JArray(new List<IJValue>(), CurrentPair.Key));

                        break;
                    case nameof(JsonParserTokens.ArrayEnd):
                        // If the value is null - it means it was a primitive type (that isn't a string), which doesn't have a token.
                        if (CurrentPair.Value == null)
                            ParseValue(e.Leading);

                        ((JArray)Parents[Parents.Count - 1]).Items.Add(CurrentPair.Value);

                        CurrentPair.Key = ((JArray)Parents[Parents.Count - 1]).Key;
                        CurrentPair.Value = Parents[Parents.Count - 1];
                        Parents.RemoveAt(Parents.Count - 1);

                        break;
                    case nameof(JsonParserTokens.String):
                        InString = true;

                        if (OnValue)
                            CurrentPair.Value = new JPrimitive(JPrimitiveType.String, e.Trailing);
                        else
                            CurrentPair.Key = e.Trailing;

                        break;
                    case nameof(JsonParserTokens.PairSeperator):
                        OnValue = true;
                        ParseValue(e.Trailing);

                        break;
                    case nameof(JsonParserTokens.ItemSeperator):
                        // For Arrays
                        if (Parents[Parents.Count - 1] is JArray)
                        {
                            // If the value is null - it means it was a primitive type (that isn't a string), which doesn't have a token.
                            if (CurrentPair.Value == null)
                                ParseValue(e.Leading);

                            ((JArray)Parents[Parents.Count - 1]).Items.Add(CurrentPair.Value);

                            CurrentPair.Value = null;
                        }
                        else if (Parents[Parents.Count - 1] is JObject)
                        {
                            ((JObject)Parents[Parents.Count - 1]).Pairs.Add(CurrentPair);
                            OnValue = false;

                            // Create a new pair
                            CurrentPair = new JPair();
                        }

                        break;
                }
            }
        }

        public void ParseValue(string value)
        {
            if (value == "" || value == "null") // Most Likely String (which will be parsed later) or null.
                CurrentPair.Value = null;
            else if (value == "false") // False
                CurrentPair.Value = new JPrimitive(JPrimitiveType.Boolean, false);
            else if (value == "true") // True
                CurrentPair.Value = new JPrimitive(JPrimitiveType.Boolean, true);
            else // Numerical
            {
                if (float.TryParse(value, out float result)) CurrentPair.Value = new JPrimitive(JPrimitiveType.Numerical, result);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            Result = new JObject();
            Parents = new List<IJValue>();
        }
    }
}

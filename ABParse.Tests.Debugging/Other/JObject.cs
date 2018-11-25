using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Debugging.Other
{
    public interface IJValue { }

    public class JObject : IJValue
    {
        public List<JPair> Pairs = new List<JPair>();
    }

    public class JArray : IJValue
    {
        public List<IJValue> Items = new List<IJValue>();
    }

    public class JPrimitive : IJValue
    {
        public object Data;
        public JPrimitiveType Type;

        public JPrimitive(JPrimitiveType type, object data)
        {
            Type = type;
            Data = data;
        }
    }

    public class JPair
    {
        public string Key;
        public IJValue Value;

        public JPair(string key) { Key = key; }
    }

    public enum JPrimitiveType
    {
        String,
        Boolean,
        Numerical
    }
}

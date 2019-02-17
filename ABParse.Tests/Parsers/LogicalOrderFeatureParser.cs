using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests.Parsers
{
    /// <summary>
    /// A parser to ensure that everything is done in an order the user understands.
    /// </summary>
    public class LogicalOrderFeatureParser : ABParser
    {
        public override bool NotifyCharacterProcessed => true;

        public List<LogicalOrderEvent> Events = new List<LogicalOrderEvent>();

        public LogicalOrderFeatureParser()
        {
            Tokens = new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
            {
                new ABParserToken("1"),
                new ABParserToken("2"),
                new ABParserToken("3")
            };
        }

        protected override void OnStart()
        {
            Events.Add(new LogicalOrderEvent(LogicalOrderEventType.Start, '\0', null));
        }

        protected override void OnEnd()
        {
            Events.Add(new LogicalOrderEvent(LogicalOrderEventType.End, '\0', null));
        }

        protected override void OnCharacterProcessed(char ch)
        {
            Events.Add(new LogicalOrderEvent(LogicalOrderEventType.Character, ch, null));
        }

        protected override void BeforeTokenProcessed(ABParserToken token)
        {
            Events.Add(new LogicalOrderEvent(LogicalOrderEventType.BeforeTokenProcessed, '\0', token));
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            Events.Add(new LogicalOrderEvent(LogicalOrderEventType.OnTokenProcessed, '\0', e.Token));
        }
    }

    /// <summary>
    /// Represents an event (token/character) found by the OCPFeatureParser.
    /// </summary>
    public class LogicalOrderEvent
    {
        public LogicalOrderEventType Type;
        public char Character;
        public ABParserToken Token;

        public LogicalOrderEvent(LogicalOrderEventType type, char character, ABParserToken token = null)
        {
            Type = type;
            Character = character;
            Token = token;
        }
    }

    public enum LogicalOrderEventType
    {
        Character,
        BeforeTokenProcessed,
        OnTokenProcessed,
        Start,
        End
    }
}

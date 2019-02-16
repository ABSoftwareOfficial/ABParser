using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABParse.Tests
{
    /// <summary>
    /// Provides base functionality for a testable parser.
    /// </summary>
    public class TestingBaseParser : ABParser
    {
        public List<string> Leads = new List<string>();
        public List<string> Trails = new List<string>();
        public List<string> Names = new List<string>();
        public List<int> Starts = new List<int>();
        public List<int> Ends = new List<int>();
        public List<int> CurrentLocations = new List<int>();

        public TestingBaseParser(ObservableCollection<ABParserToken> tokens)
        {
            Tokens = tokens;
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            Leads.Add(e.Leading);
            Trails.Add(e.Trailing);
            Names.Add(e.Token.Name);
            Starts.Add(e.StartLocation);
            Ends.Add(e.EndLocation);
            CurrentLocations.Add(CurrentLocation);
        }
    }
}

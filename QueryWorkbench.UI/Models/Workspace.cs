using System.Collections.Generic;
using System.Linq;

namespace QueryWorkbenchUI.Models {
    public class Workspace {
        public Workspace() {
            ResultPaneTitles = new List<string>();
        }
        public string Query { get; set; }
        public string Parameters { get; set; }
        public string ConnectionString { get; set; }
        public List<string> ResultPaneTitles { get; set; }

        public bool HasSameValueAs(Workspace other) {
            return Query == other.Query &&
                  Parameters == other.Parameters &&
                  ConnectionString == other.ConnectionString &&
                  ResultPaneTitles.SequenceEqual(other.ResultPaneTitles);
        }
    }
}

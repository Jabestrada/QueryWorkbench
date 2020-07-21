using System.Collections.Generic;

namespace QueryWorkbenchUI.Models {
    public class Workspace {
        public Workspace() {
            ResultPaneTitles = new List<string>();
        }
        public string Query { get; set; }
        public string Parameters { get; set; }
        public string ConnectionString { get; set; }
        public List<string> ResultPaneTitles { get; set; }
    }
}

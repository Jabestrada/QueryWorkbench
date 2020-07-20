namespace QueryWorkbenchUI.Configuration {
    public class AppState {
        private const int DEFAULT_MAX_ITEMS_COUNT = 7;

        public MRUList<string> MRUConfigList { get; set; }
        public AppState() : this(DEFAULT_MAX_ITEMS_COUNT){
        
        }

        public AppState(int maxItems)  {
            MRUConfigList = new MRUList<string>(maxItems);
        }
    }
}

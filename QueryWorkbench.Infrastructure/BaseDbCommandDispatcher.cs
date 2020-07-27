using System.Collections.Generic;
using System.Data;

namespace QueryWorkbench.Infrastructure {
    public abstract class BaseDbCommandDispatcher : IDbCommandDispatcher {
        public string ConnectionString { get; set; }

        public BaseDbCommandDispatcher(string connectionString) {
            ConnectionString = connectionString;
        }

        public abstract DataSet RunQuery(string query, Dictionary<string, object> parameters);
    }
}

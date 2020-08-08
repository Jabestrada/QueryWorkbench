using System.Collections.Generic;
using System.Data;

namespace QueryWorkbench.Infrastructure {
    public interface IDbCommandDispatcher {
        string ConnectionString { get; set; }
        DataSet RunQuery(string query, Dictionary<string, object> parameters);
        string LineCommentToken { get; }
    }
}

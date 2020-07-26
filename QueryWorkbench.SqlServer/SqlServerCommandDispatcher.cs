using QueryWorkbench.Infrastructure;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace QueryWorkbench.SqlServer {
    public class SqlServerCommandDispatcher : BaseDbCommandDispatcher {
        public SqlServerCommandDispatcher(string connectionString, Dictionary<string, object> parameters) :
                        base(connectionString, parameters) {
        }

        public override DataSet RunQuery(string command) {
            using (var sqlConn = new SqlConnection(ConnectionString)) {
                var sqlDataAdapter = new SqlDataAdapter(command, sqlConn);
                foreach (var qp in Parameters) {
                    var paramName = qp.Key;
                    if (!paramName.StartsWith("@")) {
                        paramName = "@" + paramName;
                    }
                    sqlDataAdapter.SelectCommand.Parameters.AddWithValue(paramName, qp.Value);
                }

                var ds = new DataSet();
                sqlDataAdapter.Fill(ds);
                return ds;
            }
        }
    }
}

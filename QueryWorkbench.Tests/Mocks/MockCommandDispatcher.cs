using QueryWorkbench.Infrastructure;
using System.Collections.Generic;
using System.Data;

namespace QueryWorkbench.Tests.Mocks {
    public class MockCommandDispatcher : BaseDbCommandDispatcher {
        public readonly DataSet Data;

        public MockCommandDispatcher() : this(createDummyDs()) {
            
        }

        public MockCommandDispatcher(DataSet ds) : base(string.Empty) {
            Data = ds;
        }

        public override DataSet RunQuery(string query, Dictionary<string, object> parameters) {
            return Data;
        }

        private static DataSet createDummyDs() { 
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable("Test"));
            ds.Tables[0].Columns.Add("Column1");
            ds.Tables[0].Columns.Add("Column2");
            ds.Tables[0].LoadDataRow(new object[] {"data1", "data2"}, true);
            return ds;
        }
    }
}

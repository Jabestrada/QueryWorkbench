using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryWorkbench.Infrastructure {
    public interface IDbCommandDispatcher {
        DataSet RunQuery(string query);
    }
}

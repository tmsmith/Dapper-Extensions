using System.Data;
using Dapper.Extensions.Linq.Core.Sql;
using Dapper.Extensions.Linq.Sql;

namespace Dapper.Extensions.Linq.Test.Helpers
{
    public class DatabaseInfo
    {
        public IDbConnection Connection { get; set; }
        public ISqlDialect Dialect { get; set; }
    }
}
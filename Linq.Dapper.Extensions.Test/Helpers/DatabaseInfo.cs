using System.Data;
using Linq.Dapper.Extensions.Sql;

namespace Linq.Dapper.Extensions.Test.Helpers
{
    public class DatabaseInfo
    {
        public IDbConnection Connection { get; set; }
        public ISqlDialect Dialect { get; set; }
    }
}
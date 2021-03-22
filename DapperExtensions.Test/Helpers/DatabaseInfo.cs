using DapperExtensions.Sql;
using System.Data;

namespace DapperExtensions.Test.Helpers
{
    public class DatabaseInfo
    {
        public IDbConnection Connection { get; set; }
        public ISqlDialect Dialect { get; set; }
    }
}
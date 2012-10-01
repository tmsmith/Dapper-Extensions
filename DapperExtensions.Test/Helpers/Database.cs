using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DapperExtensions.Sql;

namespace DapperExtensions.Test.Helpers
{
    public class Database
    {
        public IDbConnection Connection { get; set; }
        public ISqlDialect Dialect { get; set; }
    }
}
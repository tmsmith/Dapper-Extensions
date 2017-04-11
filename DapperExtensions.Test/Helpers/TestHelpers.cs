using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Threading;
using Dapper;
using DapperExtensions.Sql;
using MySql.Data.MySqlClient;

namespace DapperExtensions.Test.Helpers
{
    public static class TestHelpers
    {
        public static Protected TestProtected(this object obj)
        {
            return new Protected(obj);
        }
    }
}
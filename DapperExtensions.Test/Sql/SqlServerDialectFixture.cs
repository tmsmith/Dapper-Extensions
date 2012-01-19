using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Sql;
using NUnit.Framework;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    public class SqlServerDialectFixture
    {
        [Test]
        public void METHODNAME()
        {
            SqlServerDialect dialect = new SqlServerDialect();
            string sql = dialect.GetPagingSql("SELECT [client].[Individual].[FirstName], [client].[Individual].[LastName] AS [Last] FROM [client].[Individual] ORDER BY [client].[Individual].[LastName], [client].[Individual].[FirstName]", 1, 10, new Dictionary<string, object>());
            sql = dialect.GetPagingSql("SELECT [client].[Individual].[FirstName], [client].[Individual].[LastName] AS [Last] FROM [client].[Individual]", 1, 10, new Dictionary<string, object>());
        }
    }
}
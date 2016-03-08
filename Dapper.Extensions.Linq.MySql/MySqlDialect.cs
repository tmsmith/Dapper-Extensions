using System;
using System.Collections.Generic;
using System.Data;
using Dapper.Extensions.Linq.Core.Sql;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.Linq.MySql
{
    public class MySqlDialect : SqlDialectBase
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public override char OpenQuote => '`';

        public override char CloseQuote => '`';

        public override string GetIdentitySql(string tableName)
        {
            return "SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS ID";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {            
            int startValue = page * resultsPerPage;
            return GetSetSql(sql, startValue, resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            string result = string.Format("{0} LIMIT @firstResult, @maxResults", sql);
            parameters.Add("@firstResult", firstResult);
            parameters.Add("@maxResults", maxResults);
            return result;
        }

        public override string SelectLimit(string sql, int limit)
        {
            return string.Format("{0} LIMIT {1}", sql, limit);
        }

        public override string SetNolock(string sql)
        {
            return string.Concat(@"SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", sql, "; COMMIT;");
        }
    }
}
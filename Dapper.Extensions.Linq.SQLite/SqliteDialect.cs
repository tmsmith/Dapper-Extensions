using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.SQLite
{
    public class SQLiteDialect : SqlDialectBase
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        public override string GetIdentitySql(string tableName)
        {
            return "SELECT LAST_INSERT_ROWID() AS [Id]";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            int startValue = page * resultsPerPage;
            return GetSetSql(sql, startValue, resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("SQL");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var result = string.Format("{0} LIMIT @Offset, @Count", sql);
            parameters.Add("@Offset", firstResult);
            parameters.Add("@Count", maxResults);
            return result;
        }

        public override string GetColumnName(string prefix, string columnName, string alias)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(columnName, "columnName cannot be null or empty.");
            }
            var result = new StringBuilder();
            result.AppendFormat(columnName);
            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}", QuoteString(alias));
            }
            return result.ToString();
        }

        public override string SelectLimit(string sql, int limit)
        {
            return string.Format("{0} LIMIT {1}", sql, limit);
        }
    }
}

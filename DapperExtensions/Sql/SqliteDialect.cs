using System;
using System.Collections.Generic;
using System.Text;

namespace DapperExtensions.Sql
{
    public class SqliteDialect : SqlDialectBase
    {
        public override string GetIdentitySql(string tableName)
        {
            return "SELECT LAST_INSERT_ROWID() AS [Id]";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            var result = string.Format("{0} LIMIT @Offset, @Count", sql);
            var startValue = ((page - 1) * resultsPerPage);
            parameters.Add("@Offset", startValue);
            parameters.Add("@Count", resultsPerPage);
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
    }
}

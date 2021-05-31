using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperExtensions.Sql
{
    public class SqliteDialect : SqlDialectBase
    {
        public override string GetIdentitySql(string tableName)
        {
            return "SELECT LAST_INSERT_ROWID() AS [Id]";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            return GetSetSql(sql, GetStartValue(page, resultsPerPage), resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException(nameof(sql), $"{nameof(sql)} cannot be null.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
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

        public override string GetDatabaseFunctionString(DatabaseFunction databaseFunction, string columnName, string functionParameters = "")
        {
            return databaseFunction switch
            {
                DatabaseFunction.NullValue => $"IsNull({columnName}, {functionParameters})",
                DatabaseFunction.Truncate => $"Truncate({columnName})",
                _ => columnName,
            };
        }

        public override void EnableCaseInsensitive(IDbConnection connection)
        {
        }
    }
}

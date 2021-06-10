using DapperExtensions.Predicate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace DapperExtensions.Sql
{
    public class SqlServerDialect : SqlDialectBase
    {
        public override char OpenQuote
        {
            get { return '['; }
        }

        public override char CloseQuote
        {
            get { return ']'; }
        }

        public override string GetIdentitySql(string tableName)
        {
            return string.Format("SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [Id]");
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            return GetSetSql(sql, GetStartValue(page, resultsPerPage), resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException(nameof(sql), $"{nameof(sql)} cannot be null.");

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");

            if (!IsSelectSql(sql))
                throw new ArgumentException($"{nameof(sql)} must be a SELECT statement.", nameof(sql));

            if (string.IsNullOrEmpty(GetOrderByClause(sql)))
                sql = $"{sql} ORDER BY CURRENT_TIMESTAMP";

            var result = $"{sql} OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY";

            parameters.Add("@skipRows", firstResult);
            parameters.Add("@maxResults", maxResults);

            return result;
        }

        protected static string GetOrderByClause(string sql)
        {
            var orderByIndex = sql.LastIndexOf(" ORDER BY ", StringComparison.InvariantCultureIgnoreCase);
            if (orderByIndex == -1)
            {
                return null;
            }

            var result = sql.Substring(orderByIndex).Trim();

            var whereIndex = result.IndexOf(" WHERE ", StringComparison.InvariantCultureIgnoreCase);
            if (whereIndex == -1)
            {
                return result;
            }

            return result.Substring(0, whereIndex).Trim();
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

        [ExcludeFromCodeCoverage]
        public override void EnableCaseInsensitive(IDbConnection connection)
        {
        }
    }
}
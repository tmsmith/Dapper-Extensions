using DapperExtensions.Predicate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DapperExtensions.Sql
{
    public class DB2Dialect : SqlDialectBase
    {
        public override string GetIdentitySql(string tableName)
        {
            return "SELECT CAST(IDENTITY_VAL_LOCAL() AS BIGINT) AS \"ID\" FROM SYSIBM.SYSDUMMY1";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            return GetSetSql(sql, GetStartValue(page, resultsPerPage) + 1, (page * resultsPerPage), parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException(nameof(sql), $"{nameof(sql)} cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null");
            }

            if (!IsSelectSql(sql))
                throw new ArgumentException($"{nameof(sql)} must be a SELECT statement.", nameof(sql));

            var selectIndex = GetSelectEnd(sql) + 1;
            var orderByClause = GetOrderByClause(sql) ?? "ORDER BY CURRENT_TIMESTAMP";

            var projectedColumns = GetColumnNames(sql).Aggregate(new StringBuilder(), (sb, s) => (sb.Length == 0 ? sb : sb.Append(", ")).Append(GetColumnName("_TEMP", s, null)), sb => sb.ToString());
            var newSql = sql
                .Replace(" " + orderByClause, string.Empty)
                .Insert(selectIndex, string.Format("ROW_NUMBER() OVER(ORDER BY {0}) AS {1}, ", orderByClause.Substring(9), GetColumnName(null, "_ROW_NUMBER", null)));

            var result = string.Format("SELECT {0} FROM ({1}) AS \"_TEMP\" WHERE {2} BETWEEN @_pageStartRow AND @_pageEndRow",
                projectedColumns.Trim(), newSql, GetColumnName("_TEMP", "_ROW_NUMBER", null));

            parameters.Add("@_pageStartRow", firstResult);
            parameters.Add("@_pageEndRow", maxResults);
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

        protected static int GetFromStart(string sql)
        {
            return sql.IndexOf("FROM", StringComparison.InvariantCultureIgnoreCase);
        }

        protected virtual int GetSelectEnd(string sql)
        {
            return sql.IndexOf("SELECT") + (sql.TrimStart().StartsWith("SELECT DISTINCT", StringComparison.InvariantCultureIgnoreCase) ? 15 : 6);
        }

        protected virtual IList<string> GetColumnNames(string sql)
        {
            var start = GetSelectEnd(sql);
            var stop = GetFromStart(sql);
            var columnSql = sql.Substring(start, stop - start).Split(',');
            var result = new List<string>();
            foreach (string c in columnSql)
            {
                var index = c.IndexOf(" AS ", StringComparison.InvariantCultureIgnoreCase);
                if (index > 0)
                {
                    result.Add(c.Substring(index + 4).Trim());
                    continue;
                }

                var colParts = c.Split('.');
                result.Add(colParts[colParts.Length - 1].Trim());
            }

            return result;
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

        public override bool SupportsMultipleStatements => false;
    }
}

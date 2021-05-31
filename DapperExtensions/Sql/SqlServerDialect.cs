using System;
using System.Collections.Generic;
using System.Data;

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
            {
                throw new ArgumentNullException(nameof(sql), $"{nameof(sql)} cannot be null.");
            }

            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            if (String.IsNullOrEmpty(GetOrderByClause(sql)))
            {
                sql = $"{sql} ORDER BY CURRENT_TIMESTAMP";
            }

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

        protected static int GetFromStart(string sql)
        {
            var selectCount = 0;
            var words = sql.Split(' ');
            var fromIndex = 0;
            foreach (var word in words)
            {
                if (word.Equals("SELECT", StringComparison.InvariantCultureIgnoreCase))
                {
                    selectCount++;
                }

                if (word.Equals("FROM", StringComparison.InvariantCultureIgnoreCase))
                {
                    selectCount--;
                    if (selectCount == 0)
                    {
                        break;
                    }
                }

                fromIndex += word.Length + 1;
            }

            return fromIndex;
        }

        protected virtual int GetSelectEnd(string sql)
        {
            var trimmedSql = sql.TrimStart();
            if (trimmedSql.StartsWith("SELECT DISTINCT", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = trimmedSql.IndexOf("SELECT DISTINCT", StringComparison.Ordinal);
                return index + 15;
            }

            if (trimmedSql.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = trimmedSql.IndexOf("SELECT", StringComparison.Ordinal);
                return index + 6;
            }

            throw new ArgumentException("SQL must be a SELECT statement.", nameof(sql));
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

        public override void EnableCaseInsensitive(IDbConnection connection)
        {
        }
    }
}
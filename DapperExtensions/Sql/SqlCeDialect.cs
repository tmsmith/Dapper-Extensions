using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperExtensions.Sql
{
    public class SqlCeDialect : SqlDialectBase
    {
        public override char OpenQuote
        {
            get { return '['; }
        }

        public override char CloseQuote
        {
            get { return ']'; }
        }

        public override bool SupportsMultipleStatements
        {
            get { return false; }
        }

        public override bool SupportsCountOfSubquery => false;

        public override string GetTableName(string schemaName, string tableName, string alias)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName), $"{nameof(tableName)} cannot be null or empty.");
            }

            var result = new StringBuilder();
            result.Append(OpenQuote);
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                result.AppendFormat("{0}_", schemaName);
            }

            result.AppendFormat("{0}{1}", tableName, CloseQuote);


            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}{1}{2}", OpenQuote, alias, CloseQuote);
            }

            return result.ToString();
        }

        public override string GetIdentitySql(string tableName)
        {
            return "SELECT CAST(@@IDENTITY AS BIGINT) AS [Id]";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            return GetSetSql(sql, GetStartValue(page, resultsPerPage), resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            var result = string.Format("{0} OFFSET @firstResult ROWS FETCH NEXT @maxResults ROWS ONLY", sql);
            parameters.Add("@firstResult", firstResult);
            parameters.Add("@maxResults", maxResults);
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
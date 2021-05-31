using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace DapperExtensions.Sql
{
    public class MySqlDialect : SqlDialectBase
    {
        public override char OpenQuote
        {
            get { return '`'; }
        }

        public override char CloseQuote
        {
            get { return '`'; }
        }

        public override string GetIdentitySql(string tableName)
        {
            return "SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS ID";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            return GetSetSql(sql, GetStartValue(page, resultsPerPage), resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            var result = string.Format("{0} LIMIT @maxResults OFFSET @firstResult", sql);
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

        public override string GetCountSql(string sql)
        {
            var countSQL = base.GetCountSql(sql);

            var count = Regex.Matches(sql.ToUpperInvariant(), "SELECT").Count;

            if (count > 1)
            {
                return $"{countSQL} AS {OpenQuote}Total{CloseQuote}";
            }

            return countSQL;
        }
    }
}
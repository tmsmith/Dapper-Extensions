using System.Collections.Generic;
using System.Data;

namespace DapperExtensions.Sql
{
    public class PostgreSqlDialect : SqlDialectBase
    {
        public override string GetIdentitySql(string tableName)
        {
            return "SELECT LASTVAL() AS Id";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            return GetSetSql(sql, GetStartValue(page, resultsPerPage) - 1, resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            var result = string.Format("{0} LIMIT @maxResults OFFSET @pageStartRowNbr", sql);
            parameters.Add("@maxResults", maxResults);
            parameters.Add("@pageStartRowNbr", firstResult);
            return result;
        }

        public override string GetColumnName(string prefix, string columnName, string alias)
        {
            return base.GetColumnName(null, columnName, alias).ToLower();
        }

        public override string GetTableName(string schemaName, string tableName, string alias)
        {
            return base.GetTableName(schemaName, tableName, alias).ToLower();
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

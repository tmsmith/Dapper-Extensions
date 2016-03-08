using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.Sql
{
    public class PostgreSqlDialect : SqlDialectBase
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
        }

        public override string GetIdentitySql(string tableName)
        {
            return "SELECT LASTVAL() AS Id";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            int startValue = page * resultsPerPage;
            return GetSetSql(sql, startValue, resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            string result = string.Format("{0} LIMIT @firstResult OFFSET @pageStartRowNbr", sql);            
            parameters.Add("@firstResult", firstResult);
            parameters.Add("@maxResults", maxResults);
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

        public override string SelectLimit(string sql, int limit)
        {
            return string.Format("{0} LIMIT {1}", sql, limit);
        }

        public override string SetNolock(string sql)
        {
            throw new NotSupportedException();
        }
    }

}
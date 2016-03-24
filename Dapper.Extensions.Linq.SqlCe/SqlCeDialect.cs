﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Text;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.SqlCe
{
    public class SqlCeDialect : SqlDialectBase
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new SqlCeConnection(connectionString);
        }

        public override char OpenQuote => '[';

        public override char CloseQuote => ']';

        public override bool SupportsMultipleStatements => false;

        public override string GetTableName(string schemaName, string tableName, string alias)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("TableName");
            }

            StringBuilder result = new StringBuilder();
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

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            int startValue = (page * resultsPerPage);
            return GetSetSql(sql, startValue, resultsPerPage, parameters);
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            string result = string.Format("{0} OFFSET @firstResult ROWS FETCH NEXT @maxResults ROWS ONLY", sql);
            parameters.Add("@firstResult", firstResult);
            parameters.Add("@maxResults", maxResults);
            return result;
        }

        public override string SelectLimit(string sql, int limit)
        {
            const string searchFor = "SELECT ";
            return sql.Insert(sql.IndexOf(searchFor, StringComparison.OrdinalIgnoreCase) + searchFor.Length, string.Format("TOP ({0}) ", limit));
        }

        public override string SetNolock(string sql)
        {
            throw new NotSupportedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override bool RunIdentityInsertAsBatch
        {
            get { return false; }
        }

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
            string result = string.Format("{0} OFFSET @pageStartRowNbr ROWS FETCH NEXT @resultsPerPage ROWS ONLY", sql);
            int startValue = (page * resultsPerPage);
            parameters.Add("@pageStartRowNbr", startValue);
            parameters.Add("@resultsPerPage", resultsPerPage);
            return result;
        }
    }
}
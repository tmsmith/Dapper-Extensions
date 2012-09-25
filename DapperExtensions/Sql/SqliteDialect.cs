using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperExtensions.Sql
{
    [Obsolete("Not ready from primetime - use at your own risk", false)]
    public class SqliteDialect : SqlDialectBase
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
            get
            {
                return false;
            }
        }

        public override string GetIdentitySql(string tableName)
        {
            string name = UnQuoteString(tableName).ToLower();
            return string.Format(string.Format("SELECT CAST([seq] AS INTEGER) AS [Id] FROM sqlite_sequence WHERE LOWER(name) = '{0}'", name));
        }

        public override string GetColumnName(string prefix, string columnName, string alias)
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat(QuoteString(columnName));

            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}", QuoteString(alias));
            }

            return result.ToString();
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            string result = string.Format("{0} LIMIT @pageStartRowNbr, @resultsPerPage", sql);
            int startValue = ((page - 1) * resultsPerPage);
            parameters.Add("@pageStartRowNbr", startValue);
            parameters.Add("@resultsPerPage", resultsPerPage);
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperExtensions
{
    public interface ISqlDialect
    {
        char OpenQuote { get; }
        char CloseQuote { get; }
        string GetTableName(string schemaName, string tableName, string alias);
        string GetColumnName(string prefix, string columnName, string alias);
        string GetIdentitySql(string tableName);
        string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters);
        bool IsQuoted(string value);
        string QuoteString(string value);
    }

    public abstract class SqlDialectBase : ISqlDialect
    {
        public virtual char OpenQuote
        {
            get { return '"'; }
        }

        public virtual char CloseQuote
        {
            get { return '"'; }
        }

        public virtual string GetTableName(string schemaName, string tableName, string alias)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(tableName, "tableName cannot be null or empty.");
            }

            StringBuilder result = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                result.AppendFormat(QuoteString(schemaName) + ".");
            }

            result.AppendFormat(QuoteString(tableName));

            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}", QuoteString(alias));
            }
            return result.ToString();
        }

        public virtual string GetColumnName(string prefix, string columnName, string alias)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(columnName, "columnName cannot be null or empty.");
            }

            StringBuilder result = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                result.AppendFormat(QuoteString(prefix) + ".");
            }

            result.AppendFormat(QuoteString(columnName));

            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}", QuoteString(alias));
            }

            return result.ToString();
        }

        public abstract string GetIdentitySql(string tableName);
        public abstract string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters);

        public virtual bool IsQuoted(string value)
        {
            if (value.Trim()[0] == OpenQuote)
            {
                return value.Trim().Last() == CloseQuote;
            }

            return false;
        }

        public virtual string QuoteString(string value)
        {
            return IsQuoted(value) ? value : string.Format("{0}{1}{2}", OpenQuote, value.Trim(), CloseQuote);
        }
    }

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
            return string.Format("SELECT IDENT_CURRENT('{0}') AS [Id]", tableName);
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            int selectIndex = GetSelectEnd(sql) + 1;
            string orderByClause = GetOrderByClause(sql);
            if (orderByClause == null)
            {
                orderByClause = " ORDER BY CURRENT_TIMESTAMP";
            }


            string projectedColumns = GetColumnNames(sql).Aggregate(new StringBuilder(), (sb, s) => (sb.Length == 0 ? sb : sb.Append(", ")).Append(GetColumnName("_proj", s, null)), sb => sb.ToString());
            string newSql = sql
                .Replace(orderByClause, string.Empty)
                .Insert(selectIndex, string.Format("ROW_NUMBER() OVER(ORDER BY {0}) AS {1}, ", orderByClause.Substring(10), GetColumnName(null, "_row_number", null)));

            string result = string.Format("SELECT TOP({0}) {1} FROM ({2}) [_proj] WHERE {3} >= @_pageStartRow ORDER BY {3}",
                resultsPerPage, projectedColumns.Trim(), newSql, GetColumnName("_proj", "_row_number", null));

            int startValue = (page * resultsPerPage) + 1;
            parameters.Add("@_pageStartRow", startValue);
            return result;
        }

        protected string GetOrderByClause(string sql)
        {
            int orderByIndex = sql.LastIndexOf(" ORDER BY ", StringComparison.InvariantCultureIgnoreCase);
            if (orderByIndex > 0)
            {
                return sql.Substring(orderByIndex);
            }

            return null;
        }

        protected int GetFromStart(string sql)
        {
            int selectCount = 0;
            string[] words = sql.Split(' ');
            int fromIndex = 0;
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

        protected int GetSelectEnd(string sql)
        {
            if (sql.StartsWith("SELECT DISTINCT", StringComparison.InvariantCultureIgnoreCase))
            {
                return 15;
            }

            if (sql.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase))
            {
                return 6;
            }

            throw new ArgumentException("SQL must be a SELECT statement.", "sql");
        }

        protected IList<string> GetColumnNames(string sql)
        {
            int start = GetSelectEnd(sql);
            int stop = GetFromStart(sql);
            string[] columnSql = sql.Substring(start, stop - start).Split(',');
            List<string> result = new List<string>();
            foreach (string c in columnSql)
            {
                int index = c.IndexOf(" AS ", StringComparison.InvariantCultureIgnoreCase);
                if (index > 0)
                {
                    result.Add(c.Substring(index + 4).Trim());
                    continue;
                }

                string[] colParts = c.Split('.');
                result.Add(colParts[colParts.Length - 1].Trim());
            }

            return result;
        }
    }

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

        public override string GetTableName(string schemaName, string tableName, string alias)
        {
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
            return "SELECT @@IDENTITY AS [Id]";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            string result = string.Format("{0} OFFSET @pageStartRowNbr ROWS FETCH NEXT @resultsPerPage ROWS ONLY", sql);
            int startValue = ((page - 1) * resultsPerPage);
            parameters.Add("@pageStartRowNbr", startValue);
            parameters.Add("@resultsPerPage", resultsPerPage);
            return result;
        }
    }

    [Obsolete("Not ready from primetime - use at your own risk", false)]
    public class SqlMySqlDialect : SqlDialectBase
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
            return "SELECT LAST_INSERT_ID()";
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
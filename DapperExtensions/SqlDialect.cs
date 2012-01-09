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
        string GetPagingSql(string columns, string orderBy, string sql, int page, int resultsPerPage, IDictionary<string, object> parameters);
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
            StringBuilder result = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                result.AppendFormat("{0}{1}{2}.", OpenQuote, schemaName, CloseQuote);
            }

            result.AppendFormat("{0}{1}{2}", OpenQuote, tableName, CloseQuote);

            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}{1}{2}", OpenQuote, alias, CloseQuote);
            }
            return result.ToString();
        }

        public virtual string GetColumnName(string prefix, string columnName, string alias)
        {
            StringBuilder result = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                result.Append(prefix + ".");
            }

            result.AppendFormat("{0}{1}{2}", OpenQuote, columnName, CloseQuote);

            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}{1}{2}", OpenQuote, alias, CloseQuote);
            }

            return result.ToString();
        }

        public abstract string GetIdentitySql(string tableName);
        public abstract string GetPagingSql(string columns, string orderBy, string sql, int page, int resultsPerPage, IDictionary<string, object> parameters);
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

        public override string GetPagingSql(string columns, string orderBy, string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            string result = string.Format("SELECT {0} FROM ({1} ORDER BY {2}) proj WHERE proj.[RowNbr] BETWEEN @pageStartRowNbr AND @pageStopRowNbr ORDER BY proj.[RowNbr]",
                columns, sql, orderBy);

            int startValue = (page * resultsPerPage) + 1;
            parameters.Add("@pageStartRowNbr", startValue);
            parameters.Add("@pageStopRowNbr", startValue + resultsPerPage);
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

        public override string GetPagingSql(string columns, string orderBy, string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            string result = string.Format("{0} ORDER BY {1} OFFSET @pageStartRowNbr ROWS FETCH NEXT @resultsPerPage ROWS ONLY", sql, orderBy);
            int startValue = ((page - 1) * resultsPerPage);
            parameters.Add("@pageStartRowNbr", startValue);
            parameters.Add("@resultsPerPage", resultsPerPage);
            return result;
        }
    }
}
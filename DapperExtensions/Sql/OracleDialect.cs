using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperExtensions.Sql
{
    public class OracleDialect : SqlDialectBase
    {
        public OracleDialect()
        {
            //Dapper.SqlMapper.AddTypeMap(typeof(Guid), DbType.String);
            //Dapper.SqlMapper.AddTypeMap(typeof(bool), DbType.Int16);
        }

        public override string GetIdentitySql(string tableName)
        {
            throw new System.NotImplementedException();
        }

        public override bool SupportsMultipleStatements
        {
            get { return false; }
        }

        //from Simple.Data.Oracle implementation https://github.com/flq/Simple.Data.Oracle/blob/master/Simple.Data.Oracle/OraclePager.cs
        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            var toSkip = page * resultsPerPage;
            var topLimit = (page + 1) * resultsPerPage;
         
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            sb.AppendLine("SELECT \"_ss_dapper_1_\".*, ROWNUM RNUM FROM (");
            sb.Append(sql);
            sb.AppendLine(") \"_ss_dapper_1_\"");
            sb.AppendLine("WHERE ROWNUM <= :topLimit) \"_ss_dapper_2_\" ");
            sb.AppendLine("WHERE \"_ss_dapper_2_\".RNUM > :toSkip");

            parameters.Add(":topLimit", topLimit);
            parameters.Add(":toSkip", toSkip);

            return sb.ToString();
        }

        public override string QuoteString(string value)
        {
            if (value != null && value[0]=='`')
            {
                return string.Format("{0}{1}{2}", OpenQuote, value.Substring(1, value.Length - 2), CloseQuote);
            }
            return value.ToUpper();
        }

        public override char ParameterPrefix
        {
            get { return ':'; }
        }

        public override char OpenQuote
        {
            get { return '"'; }
        }

        public override char CloseQuote
        {
            get { return '"'; }
        }
    }
}
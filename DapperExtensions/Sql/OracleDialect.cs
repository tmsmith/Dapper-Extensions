using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DapperExtensions.Sql
{
    public class OracleDialect : SqlDialectBase
    {
        public OracleDialect() { }

        public override string GetIdentitySql(string tableName)
        {
            throw new System.NotImplementedException("Oracle does not support get last inserted identity.");
        }

        public override bool SupportsMultipleStatements
        {
            get { return false; }
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            var toSkip = GetStartValue(page, resultsPerPage);
            var topLimit = toSkip + resultsPerPage;

            string setCompare(string src, string left, string right)
            {
                var fields = src.Split(new string[] { ", " }, StringSplitOptions.None);
                var result = "";

                if (fields.Length > 1)
                {
                    result = fields.Aggregate((prior, next) =>
                    {
                        string aliasedPrior;

                        if (prior.Contains($"{left}."))
                        {
                            aliasedPrior = prior;
                        }
                        else
                        {
                            aliasedPrior = $"{left}.{prior} = {right}.{prior}";
                        }

                        return $"{aliasedPrior} and {left}.{next} = {right}.{next}";
                    });
                }
                else
                {
                    result = $"{left}.{fields[0]} = {right}.{fields[0]}";
                }

                return result;
            }

            //TODO: Melhorar a forma de pegar o line number para reduzir o custo da consulta
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");

            sb.AppendLine("SELECT ss_dapper_1.*, liner.LINE_NUMBER FROM (");
            sb.Append(sql);
            sb.AppendLine(") ss_dapper_1 ");
            sb.AppendLine($"inner join (select {partitionBy}, ROW_NUMBER() OVER (ORDER BY {partitionBy} ASC) LINE_NUMBER from (");
            sb.AppendLine($"select distinct {partitionBy} from ({sql}))) liner on {setCompare(partitionBy, "liner", "ss_dapper_1")}");
            sb.AppendLine(") ss_dapper_2 ");
            sb.AppendLine("WHERE ss_dapper_2.line_number > :toSkip AND ss_dapper_2.line_number <= :topLimit");

            parameters.Add(":topLimit", topLimit);
            parameters.Add(":toSkip", toSkip);

            return sb.ToString();
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            sb.AppendLine("SELECT ss_dapper_1.*, ROWNUM RNUM FROM (");
            sb.Append(sql);
            sb.AppendLine(") ss_dapper_1");
            sb.AppendLine("WHERE ROWNUM <= :topLimit) ss_dapper_2 ");
            sb.AppendLine("WHERE ss_dapper_2.RNUM > :toSkip");

            parameters.Add(":topLimit", maxResults + firstResult);
            parameters.Add(":toSkip", firstResult);

            return sb.ToString();
        }

        public override string QuoteString(string value)
        {
            if (value[0] == '`')
            {
                value = value.Substring(1, value.Length - 2);
            }
            return value;
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

        public override string GetDatabaseFunctionString(DatabaseFunction databaseFunction, string columnName, string functionParameters = "")
        {
            return databaseFunction switch
            {
                DatabaseFunction.NullValue => $"nvl({columnName}, {functionParameters})",
                DatabaseFunction.Truncate => $"Trunc({columnName})",
                _ => columnName,
            };
        }

        public override void EnableCaseInsensitive(IDbConnection connection)
        {
            var conn = connection as OracleConnection;
            var info = conn.GetSessionInfo();
            info.Sort = "BINARY_CI"; // NLS_SORT:
            info.Comparison = "LINGUISTIC"; // NLS_COMP:
            conn.SetSessionInfo(info);
        }
    }
}
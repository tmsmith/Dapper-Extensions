using DapperExtensions.Predicate;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DapperExtensions.Sql
{
    public class OracleDialect : SqlDialectBase
    {
        public OracleDialect() { }

        public override string GetIdentitySql(string tableName)
        {
            throw new NotImplementedException("Oracle does not support get last inserted identity.");
        }

        public override bool SupportsMultipleStatements
        {
            get { return false; }
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
        {
            if (string.IsNullOrEmpty(partitionBy))
                throw new ArgumentNullException(nameof(partitionBy), $"{nameof(partitionBy)} cannot be null.");

            if (string.IsNullOrWhiteSpace(partitionBy))
                throw new ArgumentNullException(nameof(partitionBy), $"{nameof(partitionBy)} cannot be null.");

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

            return string.Format(GetSetSql(sql, toSkip, topLimit, parameters), partitionBy, partitionBy, partitionBy, sql, setCompare(partitionBy, "liner", "ss_dapper_1"));
        }

        public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException(nameof(sql), $"{nameof(sql)} cannot be null.");

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql), $"{nameof(sql)} cannot be null.");

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");

            if (!IsSelectSql(sql))
                throw new ArgumentException($"{nameof(sql)} must be a SELECT statement.", nameof(sql));

            //TODO: Melhorar a forma de pegar o line number para reduzir o custo da consulta
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");

            sb.AppendLine("SELECT ss_dapper_1.*, liner.LINE_NUMBER FROM (");
            sb.Append(sql);
            sb.AppendLine(") ss_dapper_1 ");
            sb.AppendLine("inner join (select {0}, ROW_NUMBER() OVER (ORDER BY {1} ASC) LINE_NUMBER from (");
            sb.AppendLine("select distinct {2} from ({3}))) liner on {4}");
            sb.AppendLine(") ss_dapper_2 ");
            sb.AppendLine("WHERE ss_dapper_2.line_number > :toSkip AND ss_dapper_2.line_number <= :topLimit");

            parameters.Add(":topLimit", maxResults);
            parameters.Add(":toSkip", firstResult);

            return sb.ToString();
        }

        public override string QuoteString(string value)
        {
            if (value[0] == '`')
            {
                value = value.Substring(1);
            }

            if (value.EndsWith("`"))
            {
                value = value.Substring(0, value.Length - 1);
            }

            return $"{OpenQuote}{value}{CloseQuote}";
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
            get { return OpenQuote; }
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

        public override string GetTableName(string schemaName, string tableName, string alias)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName), $"{nameof(tableName)} cannot be null or empty.");
            }

            var result = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                result.AppendFormat(schemaName + ".");
            }

            result.AppendFormat(tableName);

            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" {0}", alias);
            }
            return result.ToString();
        }

        public override string GetColumnName(string prefix, string columnName, string alias)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName), $"{nameof(columnName)} cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentNullException(nameof(columnName), $"{nameof(columnName)} cannot be null or empty.");

            var result = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                result.AppendFormat(prefix + ".");
            }

            result.AppendFormat(columnName);

            if (!string.IsNullOrWhiteSpace(alias))
            {
                result.AppendFormat(" AS {0}", alias);
            }

            return result.ToString();
        }
    }
}
using System.Collections.Generic;
using System.Data;

namespace Dapper.Extensions.Linq.Core.Sql
{
    public interface ISqlDialect
    {
        IDbConnection GetConnection(string connectionString);
        char OpenQuote { get; }
        char CloseQuote { get; }
        string BatchSeperator { get; }
        bool SupportsMultipleStatements { get; }
        char ParameterPrefix { get; }
        string EmptyExpression { get; }
        string GetTableName(string schemaName, string tableName, string alias);
        string GetColumnName(string prefix, string columnName, string alias);
        string GetIdentitySql(string tableName);
        string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters);
        string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters);
        string SelectLimit(string sql, int limit);
        string SetNolock(string sql);
        bool IsQuoted(string value);
        string QuoteString(string value);
    }
}
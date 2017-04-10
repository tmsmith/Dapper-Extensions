using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using Xunit;

namespace DapperExtensions.Test.Sql
{
    
    public class SqlServerDialectFixture
    {
        public abstract class SqlServerDialectFixtureBase
        {
            protected SqlServerDialect Dialect;

            public SqlServerDialectFixtureBase()
            {
                Dialect = new SqlServerDialect();
            }
        }

        
        public class Properties : SqlServerDialectFixtureBase
        {
            [Fact]
            public void CheckSettings()
            {
                Assert.Equal('[', Dialect.OpenQuote);
                Assert.Equal(']', Dialect.CloseQuote);
                Assert.Equal(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.Equal('@', Dialect.ParameterPrefix);
                Assert.True(Dialect.SupportsMultipleStatements);
            }
        }

        
        public class GetPagingSqlMethod : SqlServerDialectFixtureBase
        {
            [Fact]
            public void NullSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(null, 0, 10, new Dictionary<string, object>()));
                Assert.Equal("SQL", ex.ParamName);
                Assert.True(ex.Message.Contains("cannot be null"));
            }

            [Fact]
            public void EmptySql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(string.Empty, 0, 10, new Dictionary<string, object>()));
                Assert.Equal("SQL", ex.ParamName);
                Assert.True(ex.Message.Contains("cannot be null"));
            }

            [Fact]
            public void NullParameters_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql("SELECT [schema].[column] FROM [schema].[table]", 0, 10, null));
                Assert.Equal("Parameters", ex.ParamName);
                Assert.True(ex.Message.Contains("cannot be null"));
            }

            [Fact]
            public void Select_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                string sql = "SELECT TOP(10) [_proj].[column] FROM (SELECT ROW_NUMBER() OVER(ORDER BY CURRENT_TIMESTAMP) AS [_row_number], [column] FROM [schema].[table]) [_proj] WHERE [_proj].[_row_number] >= @_pageStartRow ORDER BY [_proj].[_row_number]";
                var result = Dialect.GetPagingSql("SELECT [column] FROM [schema].[table]", 0, 10, parameters);
                Assert.Equal(sql, result);
                Assert.Equal(1, parameters.Count);
                Assert.Equal(parameters["@_pageStartRow"], 1);
            }

            [Fact]
            public void SelectDistinct_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                string sql = "SELECT TOP(10) [_proj].[column] FROM (SELECT DISTINCT ROW_NUMBER() OVER(ORDER BY CURRENT_TIMESTAMP) AS [_row_number], [column] FROM [schema].[table]) [_proj] WHERE [_proj].[_row_number] >= @_pageStartRow ORDER BY [_proj].[_row_number]";
                var result = Dialect.GetPagingSql("SELECT DISTINCT [column] FROM [schema].[table]", 0, 10, parameters);
                Assert.Equal(sql, result);
                Assert.Equal(1, parameters.Count);
                Assert.Equal(parameters["@_pageStartRow"], 1);
            }

            [Fact]
            public void SelectOrderBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                string sql = "SELECT TOP(10) [_proj].[column] FROM (SELECT ROW_NUMBER() OVER(ORDER BY [column] DESC) AS [_row_number], [column] FROM [schema].[table]) [_proj] WHERE [_proj].[_row_number] >= @_pageStartRow ORDER BY [_proj].[_row_number]";
                var result = Dialect.GetPagingSql("SELECT [column] FROM [schema].[table] ORDER BY [column] DESC", 0, 10, parameters);
                Assert.Equal(sql, result);
                Assert.Equal(1, parameters.Count);
                Assert.Equal(parameters["@_pageStartRow"], 1);
            }
        }

        
        public class GetOrderByClauseMethod : SqlServerDialectFixtureBase
        {
            [Fact]
            public void NoOrderBy_Returns()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table");
                Assert.Null(result);
            }

            [Fact]
            public void OrderBy_ReturnsItemsAfterClause()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table ORDER BY Column1 ASC, Column2 DESC");
                Assert.Equal("ORDER BY Column1 ASC, Column2 DESC", result);
            }

            [Fact]
            public void OrderByWithWhere_ReturnsOnlyOrderBy()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table ORDER BY Column1 ASC, Column2 DESC WHERE Column1 = 'value'");
                Assert.Equal("ORDER BY Column1 ASC, Column2 DESC", result);
            }
        }
    }
}
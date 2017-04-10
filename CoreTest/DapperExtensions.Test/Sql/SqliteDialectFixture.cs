using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using Xunit;

namespace DapperExtensions.Test.Sql
{
    
    public class SqliteDialectFixture
    {
        public abstract class SqliteDialectFixtureBase
        {
            protected SqliteDialect Dialect;

            public SqliteDialectFixtureBase()
            {
                Dialect = new SqliteDialect();
            }
        }

        
        public class Properties : SqliteDialectFixtureBase
        {
            [Fact]
            public void CheckSettings()
            {
                Assert.Equal('"', Dialect.OpenQuote);
                Assert.Equal('"', Dialect.CloseQuote);
                Assert.Equal(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.Equal('@', Dialect.ParameterPrefix);
                Assert.True(Dialect.SupportsMultipleStatements);
            }
        }

        
        public class GetPagingSqlMethod : SqliteDialectFixtureBase
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
                string sql = "SELECT [column] FROM [schema].[table] LIMIT @Offset, @Count";
                var result = Dialect.GetPagingSql("SELECT [column] FROM [schema].[table]", 0, 10, parameters);
                Assert.Equal(sql, result);
                Assert.Equal(2, parameters.Count);
                Assert.Equal(parameters["@Offset"], 0);
                Assert.Equal(parameters["@Count"], 10);
            }
        }
    }
}
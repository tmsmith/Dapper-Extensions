using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Sql;
using Xunit;

namespace DapperExtensions.Test.Sql
{
    public class SqlCeDialectFixture
    {
        public abstract class SqlCeDialectFixtureBase
        {
            protected SqlCeDialect Dialect;

            public SqlCeDialectFixtureBase()
            {
                Dialect = new SqlCeDialect();
            }
        }

        public class Properties : SqlCeDialectFixtureBase
        {
            [Fact]
            public void CheckSettings()
            {
                Assert.Equal('[', Dialect.OpenQuote);
                Assert.Equal(']', Dialect.CloseQuote);
                Assert.Equal(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.Equal('@', Dialect.ParameterPrefix);
                Assert.False(Dialect.SupportsMultipleStatements);
            }
        }

        
        public class GetTableNameMethod : SqlCeDialectFixtureBase
        {
            [Fact]
            public void NullTableName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetTableName(null, null, null));
                Assert.Equal("TableName", ex.ParamName);
                Assert.True(ex.Message.Contains("cannot be null"));
            }

            [Fact]
            public void EmptyTableName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetTableName(null, string.Empty, null));
                Assert.Equal("TableName", ex.ParamName);
                Assert.True(ex.Message.Contains("cannot be null"));
            }

            [Fact]
            public void TableNameOnly_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetTableName(null, "foo", null);
                Assert.Equal("[foo]", result);
            }

            [Fact]
            public void SchemaAndTable_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetTableName("bar", "foo", null);
                Assert.Equal("[bar_foo]", result);
            }

            [Fact]
            public void AllParams_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetTableName("bar", "foo", "al");
                Assert.Equal("[bar_foo] AS [al]", result);
            }
        }
    }
}
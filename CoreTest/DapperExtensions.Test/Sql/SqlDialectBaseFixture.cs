using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Sql;
using Xunit;

namespace DapperExtensions.Test.Sql
{
    
    public class SqlDialectBaseFixture
    {
        public abstract class SqlDialectBaseFixtureBase
        {
            protected TestDialect Dialect;

            public SqlDialectBaseFixtureBase()
            {
                Dialect = new TestDialect();
            } 
        }

        
        public class Properties : SqlDialectBaseFixtureBase
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

        
        public class IsQuotedMethod : SqlDialectBaseFixtureBase
        {
            [Fact]
            public void WithQuotes_ReturnsTrue()
            {
                Assert.True(Dialect.IsQuoted("\"foo\""));
            }

            [Fact]
            public void WithOnlyStartQuotes_ReturnsFalse()
            {
                Assert.False(Dialect.IsQuoted("\"foo"));
            }

            [Fact]
            public void WithOnlyCloseQuotes_ReturnsFalse()
            {
                Assert.False(Dialect.IsQuoted("foo\""));
            }
        }
        
        
        public class QuoteStringMethod : SqlDialectBaseFixtureBase
        {
            [Fact]
            public void WithNoQuotes_AddsQuotes()
            {
                Assert.Equal("\"foo\"", Dialect.QuoteString("foo"));
            }

            [Fact]
            public void WithStartQuote_AddsQuotes()
            {
                Assert.Equal("\"\"foo\"", Dialect.QuoteString("\"foo"));
            }

            [Fact]
            public void WithCloseQuote_AddsQuotes()
            {
                Assert.Equal("\"foo\"\"", Dialect.QuoteString("foo\""));
            }

            [Fact]
            public void WithBothQuote_DoesNotAddQuotes()
            {
                Assert.Equal("\"foo\"", Dialect.QuoteString("\"foo\""));
            }
        }

        
        public class UnQuoteStringMethod : SqlDialectBaseFixtureBase
        {
            [Fact]
            public void WithNoQuotes_AddsQuotes()
            {
                Assert.Equal("foo", Dialect.UnQuoteString("foo"));
            }

            [Fact]
            public void WithStartQuote_AddsQuotes()
            {
                Assert.Equal("\"foo", Dialect.UnQuoteString("\"foo"));
            }

            [Fact]
            public void WithCloseQuote_AddsQuotes()
            {
                Assert.Equal("foo\"", Dialect.UnQuoteString("foo\""));
            }

            [Fact]
            public void WithBothQuote_DoesNotAddQuotes()
            {
                Assert.Equal("foo", Dialect.UnQuoteString("\"foo\""));
            }
        }

        
        public class GetTableNameMethod : SqlDialectBaseFixtureBase
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
                Assert.Equal("\"foo\"", result);
            }

            [Fact]
            public void SchemaAndTable_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetTableName("bar", "foo", null);
                Assert.Equal("\"bar\".\"foo\"", result);
            }

            [Fact]
            public void AllParams_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetTableName("bar", "foo", "al");
                Assert.Equal("\"bar\".\"foo\" AS \"al\"", result);
            }

            [Fact]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                string result = Dialect.GetTableName("\"bar\"", "\"foo\"", "\"al\"");
                Assert.Equal("\"bar\".\"foo\" AS \"al\"", result);
            }
        }

        
        public class GetColumnNameMethod : SqlDialectBaseFixtureBase
        {
            [Fact]
            public void NullColumnName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetColumnName(null, null, null));
                Assert.Equal("ColumnName", ex.ParamName);
                Assert.True(ex.Message.Contains("cannot be null"));
            }

            [Fact]
            public void EmptyColumnName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetColumnName(null, string.Empty, null));
                Assert.Equal("ColumnName", ex.ParamName);
                Assert.True(ex.Message.Contains("cannot be null"));
            }

            [Fact]
            public void ColumnNameOnly_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetColumnName(null, "foo", null);
                Assert.Equal("\"foo\"", result);
            }

            [Fact]
            public void PrefixColumnName_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetColumnName("bar", "foo", null);
                Assert.Equal("\"bar\".\"foo\"", result);
            }

            [Fact]
            public void AllParams_ReturnsProperlyQuoted()
            {
                string result = Dialect.GetColumnName("bar", "foo", "al");
                Assert.Equal("\"bar\".\"foo\" AS \"al\"", result);
            }

            [Fact]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                string result = Dialect.GetColumnName("\"bar\"", "\"foo\"", "\"al\"");
                Assert.Equal("\"bar\".\"foo\" AS \"al\"", result);
            }
        }

        public class TestDialect : SqlDialectBase
        {
            public override string GetIdentitySql(string tableName)
            {
                throw new NotImplementedException();
            }

            public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
            {
                throw new NotImplementedException();
            }

            public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}
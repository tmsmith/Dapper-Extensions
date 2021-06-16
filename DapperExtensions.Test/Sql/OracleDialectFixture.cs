using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class OracleDialectFixture
    {
        public abstract class OracleDialectFixtureBase
        {
            protected OracleDialect Dialect;

            [SetUp]
            public void Setup()
            {
                Dialect = new OracleDialect();
            }
        }

        [TestFixture]
        public class DatabaseFunctions : OracleDialectFixtureBase
        {
            [Test]
            public void DatabaseFunctionTests()
            {
                Assert.IsTrue("foo".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.None, "foo"), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue("Nvl(foo, newFoo)".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.NullValue, "foo", "newFoo"), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue("Trunc(foo)".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.Truncate, "foo"), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestFixture]
        public class Properties : OracleDialectFixtureBase
        {
            [Test]
            public void CheckSettings()
            {
                Assert.AreEqual('"', Dialect.OpenQuote);
                Assert.AreEqual('"', Dialect.CloseQuote);
                Assert.AreEqual(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.AreEqual(':', Dialect.ParameterPrefix);
                Assert.IsFalse(Dialect.SupportsMultipleStatements);
            }
        }

        [TestFixture]
        public class GetPagingSqlMethod : OracleDialectFixtureBase
        {
            [Test]
            public void NullSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(null, 0, 10, new Dictionary<string, object>(), "Empty"));
                StringAssert.AreEqualIgnoringCase("SQL", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void EmptySql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(string.Empty, 0, 10, new Dictionary<string, object>(), "Empty"));
                StringAssert.AreEqualIgnoringCase("SQL", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void WhitespaceSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql("  ", 0, 10, new Dictionary<string, object>(), "Empty"));
                StringAssert.AreEqualIgnoringCase("SQL", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void NullParameters_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql("SELECT \"SCHEMA\".\"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 0, 10, null, "Empty"));
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void NotSelect_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentException>(() => Dialect.GetPagingSql("INSERT INTO TABLE (ID) VALUES (1)", 1, 10, new Dictionary<string, object>(), "Empty"));
                StringAssert.AreEqualIgnoringCase("SQL", ex.ParamName);
                StringAssert.Contains("must be a SELECT statement", ex.Message);
            }

            [Test]
            public void NullPartitionBy_ThrowsException()
            {
                Action action = () => Dialect.GetPagingSql("INSERT INTO TABLE (ID) VALUES (1)", 1, 10, new Dictionary<string, object>(), null);

                action.Should().Throw<ArgumentException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("partitionBy", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void EmptyPartitionBy_ThrowsException()
            {
                Action action = () => Dialect.GetPagingSql("INSERT INTO TABLE (ID) VALUES (1)", 1, 10, new Dictionary<string, object>(), string.Empty);

                action.Should().Throw<ArgumentException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("partitionBy", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void WhitespacePartitionBy_ThrowsException()
            {
                Action action = () => Dialect.GetPagingSql("INSERT INTO TABLE (ID) VALUES (1)", 1, 10, new Dictionary<string, object>(), "  ");

                action.Should().Throw<ArgumentException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("partitionBy", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void Select_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = @"SELECT * FROM (
SELECT ss_dapper_1.*, liner.LINE_NUMBER FROM (
SELECT ""COLUMN"" FROM ""SCHEMA"".""TABLE"") ss_dapper_1 
inner join (select ""COLUMN"", ROW_NUMBER() OVER (ORDER BY ""COLUMN"" ASC) LINE_NUMBER from (
select distinct ""COLUMN"" from (SELECT ""COLUMN"" FROM ""SCHEMA"".""TABLE""))) liner on liner.""COLUMN"" = ss_dapper_1.""COLUMN""
) ss_dapper_2 
WHERE ss_dapper_2.line_number > :toSkip AND ss_dapper_2.line_number <= :topLimit
";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "\"COLUMN\"");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters[":toSkip"]);
                Assert.AreEqual(10, parameters[":topLimit"]);
            }

            [Test]
            public void SelectDistinct_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = @"SELECT * FROM (
SELECT ss_dapper_1.*, liner.LINE_NUMBER FROM (
SELECT DISTINCT ""COLUMN"" FROM ""SCHEMA"".""TABLE"") ss_dapper_1 
inner join (select ""COLUMN"", ROW_NUMBER() OVER (ORDER BY ""COLUMN"" ASC) LINE_NUMBER from (
select distinct ""COLUMN"" from (SELECT DISTINCT ""COLUMN"" FROM ""SCHEMA"".""TABLE""))) liner on liner.""COLUMN"" = ss_dapper_1.""COLUMN""
) ss_dapper_2 
WHERE ss_dapper_2.line_number > :toSkip AND ss_dapper_2.line_number <= :topLimit
";
                var result = Dialect.GetPagingSql("SELECT DISTINCT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "\"COLUMN\"");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters[":toSkip"]);
                Assert.AreEqual(10, parameters[":topLimit"]);
            }

            [Test]
            public void SelectOrderBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = @"SELECT * FROM (
SELECT ss_dapper_1.*, liner.LINE_NUMBER FROM (
SELECT ""COLUMN"" FROM ""SCHEMA"".""TABLE"" ORDER BY ""COLUMN"" DESC) ss_dapper_1 
inner join (select ""COLUMN"", ROW_NUMBER() OVER (ORDER BY ""COLUMN"" ASC) LINE_NUMBER from (
select distinct ""COLUMN"" from (SELECT ""COLUMN"" FROM ""SCHEMA"".""TABLE"" ORDER BY ""COLUMN"" DESC))) liner on liner.""COLUMN"" = ss_dapper_1.""COLUMN""
) ss_dapper_2 
WHERE ss_dapper_2.line_number > :toSkip AND ss_dapper_2.line_number <= :topLimit
";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" ORDER BY \"COLUMN\" DESC", 1, 10, parameters, "\"COLUMN\"");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters[":toSkip"]);
                Assert.AreEqual(10, parameters[":topLimit"]);
            }

            [Test]
            public void SelectMultiplePartitionBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = @"SELECT * FROM (
SELECT ss_dapper_1.*, liner.LINE_NUMBER FROM (
SELECT c1, c2, c3 FROM ""SCHEMA"".""TABLE"" ORDER BY c1, c2, c3 DESC) ss_dapper_1 
inner join (select c1, c2, c3, ROW_NUMBER() OVER (ORDER BY c1, c2, c3 ASC) LINE_NUMBER from (
select distinct c1, c2, c3 from (SELECT c1, c2, c3 FROM ""SCHEMA"".""TABLE"" ORDER BY c1, c2, c3 DESC))) liner on liner.c1 = ss_dapper_1.c1 and liner.c2 = ss_dapper_1.c2 and liner.c3 = ss_dapper_1.c3
) ss_dapper_2 
WHERE ss_dapper_2.line_number > :toSkip AND ss_dapper_2.line_number <= :topLimit
";
                var result = Dialect.GetPagingSql("SELECT c1, c2, c3 FROM \"SCHEMA\".\"TABLE\" ORDER BY c1, c2, c3 DESC", 1, 10, parameters, "c1, c2, c3");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters[":toSkip"]);
                Assert.AreEqual(10, parameters[":topLimit"]);
            }
        }

        [TestFixture]
        public class GetIdentitySqlMethod : OracleDialectFixtureBase
        {
            [Test]
            public void GetIdentity_ThrowsException()
            {
                Action action = () => Dialect.GetIdentitySql(null);

                action.Should().Throw<NotImplementedException>().WithMessage("Oracle does not support*");
            }
        }

        [TestFixture]
        public class QuoteStringMethod : OracleDialectFixtureBase
        {
            [Test]
            public void WithNoQuotes_AddsQuotes()
            {
                Dialect.QuoteString("foo").Should().Be("\"foo\"");
            }

            [Test]
            public void WithStartQuote_AddsQuotes()
            {
                Dialect.QuoteString("\"foo").Should().Be("\"\"foo\"");
            }

            [Test]
            public void WithCloseQuote_AddsQuotes()
            {
                Dialect.QuoteString("foo\"").Should().Be("\"foo\"\"");
            }

            [Test]
            public void WithBothQuote_DoesNotAddQuotes()
            {
                Dialect.QuoteString("\"foo\"").Should().Be("\"\"foo\"\"");
            }

            [Test]
            public void WithStartSingleQuote_AddsQuotes()
            {
                Dialect.QuoteString("`foo").Should().Be("\"foo\"");
            }

            [Test]
            public void WithCloseSingleQuote_AddsQuotes()
            {
                Dialect.QuoteString("foo`").Should().Be("\"foo\"");
            }

            [Test]
            public void WithBothSingleQuote_DoesNotAddQuotes()
            {
                Dialect.QuoteString("`foo`").Should().Be("\"foo\"");
            }
        }

        [TestFixture]
        public class GetTableNameMethod : OracleDialectFixtureBase
        {
            [Test]
            public void NullTableName_ThrowsException()
            {
                Action action = () => Dialect.GetTableName(null, null, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(m => m.ParamName.Equals("TableName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void EmptyTableName_ThrowsException()
            {
                Action action = () => Dialect.GetTableName(null, String.Empty, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(m => m.ParamName.Equals("TableName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void TableNameOnly_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName(null, "foo", null);
                result.Should().Be("foo");
            }

            [Test]
            public void SchemaAndTable_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName("bar", "foo", null);
                result.Should().Be("bar.foo");
            }

            [Test]
            public void AllParams_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName("bar", "foo", "al");
                result.Should().Be("bar.foo al");
            }

            [Test]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                var result = Dialect.GetTableName("\"bar\"", "\"foo\"", "\"al\"");
                result.Should().Be("\"bar\".\"foo\" \"al\"");
            }
        }

        [TestFixture]
        public class GetColumnNameMethod : OracleDialectFixtureBase
        {
            [Test]
            public void NullColumnName_ThrowsException()
            {
                Action action = () => Dialect.GetColumnName(null, null, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(m => m.ParamName.Equals("columnName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void EmptyColumnName_ThrowsException()
            {
                Action action = () => Dialect.GetColumnName(null, string.Empty, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(m => m.ParamName.Equals("columnName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void WhitespaceColumnName_ThrowsException()
            {
                Action action = () => Dialect.GetColumnName(null, "  ", null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(m => m.ParamName.Equals("columnName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void ColumnNameOnly_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName(null, "foo", null);
                result.Should().Be("foo");
            }

            [Test]
            public void PrefixColumnName_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName("bar", "foo", null);
                result.Should().Be("bar.foo");
            }

            [Test]
            public void AllParams_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName("bar", "foo", "al");
                result.Should().Be("bar.foo AS al");
            }

            [Test]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                var result = Dialect.GetColumnName("\"bar\"", "\"foo\"", "\"al\"");
                result.Should().Be("\"bar\".\"foo\" AS \"al\"");
            }
        }
    }
}
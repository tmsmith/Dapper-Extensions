using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class PostgreSqlDialectFixture
    {
        public abstract class PostgreSqlDialectFixtureBase
        {
            protected PostgreSqlDialect Dialect;

            [SetUp]
            public void Setup()
            {
                Dialect = new PostgreSqlDialect();
            }
        }

        [TestFixture]
        public class DatabaseFunctions : PostgreSqlDialectFixtureBase
        {
            [Test]
            public void DatabaseFunctionTests()
            {
                Assert.IsTrue("foo".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.None, "foo"), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue("coalesce(foo, newFoo)".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.NullValue, "foo", "newFoo"), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue("Truncate(foo)".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.Truncate, "foo"), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestFixture]
        public class Properties : PostgreSqlDialectFixtureBase
        {
            [Test]
            public void CheckSettings()
            {
                Assert.AreEqual('"', Dialect.OpenQuote);
                Assert.AreEqual('"', Dialect.CloseQuote);
                Assert.AreEqual(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.AreEqual('@', Dialect.ParameterPrefix);
                Assert.IsTrue(Dialect.SupportsMultipleStatements);
            }
        }

        [TestFixture]
        public class GetPagingSqlMethod : PostgreSqlDialectFixtureBase
        {
            [Test]
            public void NullSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(null, 0, 10, new Dictionary<string, object>(), ""));
                StringAssert.AreEqualIgnoringCase("SQL", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void EmptySql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(string.Empty, 0, 10, new Dictionary<string, object>(), ""));
                StringAssert.AreEqualIgnoringCase("SQL", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void NullParameters_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql("SELECT \"SCHEMA\".\"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 0, 10, null, ""));
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void NotSelect_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentException>(() => Dialect.GetPagingSql("INSERT INTO TABLE (ID) VALUES (1)", 1, 10, new Dictionary<string, object>(), ""));
                StringAssert.AreEqualIgnoringCase("SQL", ex.ParamName);
                StringAssert.Contains("must be a SELECT statement", ex.Message);
            }

            [Test]
            public void Select_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" LIMIT @maxResults OFFSET @pageStartRowNbr";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters["@pageStartRowNbr"]);
                Assert.AreEqual(10, parameters["@maxResults"]);
            }

            [Test]
            public void SelectDistinct_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT DISTINCT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" LIMIT @maxResults OFFSET @pageStartRowNbr";
                var result = Dialect.GetPagingSql("SELECT DISTINCT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters["@pageStartRowNbr"]);
                Assert.AreEqual(10, parameters["@maxResults"]);
            }

            [Test]
            public void SelectOrderBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" ORDER BY \"COLUMN\" DESC LIMIT @maxResults OFFSET @pageStartRowNbr";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" ORDER BY \"COLUMN\" DESC", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters["@pageStartRowNbr"]);
                Assert.AreEqual(10, parameters["@maxResults"]);
            }
        }

        [TestFixture]
        public class GetIdentitySqlMethod : PostgreSqlDialectFixtureBase
        {
            private const string _identitySql = "SELECT LASTVAL() AS Id";

            [Test]
            public void NullTableName_ReturnsSql()
            {
                var result = Dialect.GetIdentitySql(null);
                result.Should().BeEquivalentTo(_identitySql);
            }

            [Test]
            public void WithTableName_ReturnsSql()
            {
                var result = Dialect.GetIdentitySql("FooTable");
                result.Should().BeEquivalentTo(_identitySql);
            }
        }

        [TestFixture]
        public class GetTableNameMethod : PostgreSqlDialectFixtureBase
        {
            [Test]
            public void NullTableName_ThrowsException()
            {
                Action action = () => Dialect.GetTableName(null, null, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("TableName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void EmptyTableName_ThrowsException()
            {
                Action action = () => Dialect.GetTableName(null, string.Empty, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("TableName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void TableNameOnly_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName(null, "foo", null);

                result.Should().Be($"{Dialect.OpenQuote}foo{Dialect.CloseQuote}");
            }

            [Test]
            public void SchemaAndTable_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName("bar", "foo", null);

                result.Should().Be($"{Dialect.OpenQuote}bar{Dialect.CloseQuote}.{Dialect.OpenQuote}foo{Dialect.CloseQuote}");
            }

            [Test]
            public void AllParams_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName("bar", "foo", "al");
                result.Should().Be($"{Dialect.OpenQuote}bar{Dialect.CloseQuote}.{Dialect.OpenQuote}foo{Dialect.CloseQuote} {Dialect.OpenQuote}al{Dialect.CloseQuote}");
            }

            [Test]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                var result = Dialect.GetTableName("\"bar\"", "\"foo\"", "\"al\"");
                result.Should().Be($"{Dialect.OpenQuote}bar{Dialect.CloseQuote}.{Dialect.OpenQuote}foo{Dialect.CloseQuote} {Dialect.OpenQuote}al{Dialect.CloseQuote}");
            }
        }

        [TestFixture]
        public class GetColumnNameMethod : PostgreSqlDialectFixtureBase
        {
            [Test]
            public void NullColumnName_ThrowsException()
            {
                Action action = () => Dialect.GetColumnName(null, null, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("columnName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void EmptyColumnName_ThrowsException()
            {
                Action action = () => Dialect.GetColumnName(null, string.Empty, null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("columnName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void WhitespaceColumnName_ThrowsException()
            {
                Action action = () => Dialect.GetColumnName(null, "  ", null);

                action.Should().Throw<ArgumentNullException>()
                    .WithMessage("*cannot be null*")
                    .Where(ex => ex.ParamName.Equals("columnName", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void ColumnNameOnly_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName(null, "foo", null);
                result.Should().BeEquivalentTo($"{Dialect.OpenQuote}foo{Dialect.CloseQuote}");
            }

            [Test]
            public void PrefixColumnName_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName("bar", "foo", null);
                result.Should().BeEquivalentTo($"{Dialect.OpenQuote}bar{Dialect.CloseQuote}.{Dialect.OpenQuote}foo{Dialect.CloseQuote}");
            }

            [Test]
            public void AllParams_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName("bar", "foo", "al");
                result.Should().BeEquivalentTo($"{Dialect.OpenQuote}bar{Dialect.CloseQuote}.{Dialect.OpenQuote}foo{Dialect.CloseQuote} AS {Dialect.OpenQuote}al{Dialect.CloseQuote}");
            }

            [Test]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                var result = Dialect.GetColumnName("\"bar\"", "\"foo\"", "\"al\"");
                result.Should().BeEquivalentTo($"{Dialect.OpenQuote}bar{Dialect.CloseQuote}.{Dialect.OpenQuote}foo{Dialect.CloseQuote} AS {Dialect.OpenQuote}al{Dialect.CloseQuote}");
            }
        }
    }
}
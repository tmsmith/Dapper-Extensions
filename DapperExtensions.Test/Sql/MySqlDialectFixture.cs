using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class MySqlDialectFixture
    {
        public abstract class MySqlDialectFixtureBase
        {
            protected MySqlDialect Dialect;

            [SetUp]
            public void Setup()
            {
                Dialect = new MySqlDialect();
            }
        }

        [TestFixture]
        public class DatabaseFunctions : MySqlDialectFixtureBase
        {
            [Test]
            public void DatabaseFunctionTests()
            {
                Assert.IsTrue("foo".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.None, "foo"), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue("IsNull(foo, newFoo)".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.NullValue, "foo", "newFoo"), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue("Truncate(foo)".Equals(Dialect.GetDatabaseFunctionString(DatabaseFunction.Truncate, "foo"), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestFixture]
        public class Properties : MySqlDialectFixtureBase
        {
            [Test]
            public void CheckSettings()
            {
                Assert.AreEqual('`', Dialect.OpenQuote);
                Assert.AreEqual('`', Dialect.CloseQuote);
                Assert.AreEqual(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.AreEqual('@', Dialect.ParameterPrefix);
                Assert.IsTrue(Dialect.SupportsMultipleStatements);
            }
        }

        [TestFixture]
        public class GetPagingSqlMethod : MySqlDialectFixtureBase
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
                const string sql = "SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" LIMIT @maxResults OFFSET @firstResult";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters["@firstResult"]);
                Assert.AreEqual(10, parameters["@maxResults"]);
            }

            [Test]
            public void SelectDistinct_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT DISTINCT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" LIMIT @maxResults OFFSET @firstResult";
                var result = Dialect.GetPagingSql("SELECT DISTINCT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters["@firstResult"]);
                Assert.AreEqual(10, parameters["@maxResults"]);
            }

            [Test]
            public void SelectOrderBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" ORDER BY \"COLUMN\" DESC LIMIT @maxResults OFFSET @firstResult";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" ORDER BY \"COLUMN\" DESC", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(0, parameters["@firstResult"]);
                Assert.AreEqual(10, parameters["@maxResults"]);
            }
        }

        [TestFixture]
        public class GetCountSqlMethod : MySqlDialectFixtureBase
        {
            [Test]
            public void NullSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetCountSql(null));
                Assert.AreEqual("sql", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void WhitespaceSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetCountSql("  "));
                Assert.AreEqual("sql", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void TableName_ReturnsProperSql()
            {
                const string sql = "TABLE";
                var result = Dialect.GetCountSql(sql);
                Assert.AreEqual($"SELECT COUNT(*) AS {Dialect.OpenQuote}Total{Dialect.CloseQuote} FROM {sql}", result);
            }

            [Test]
            public void SubSelect_ReturnsProperSql()
            {
                const string sql = "SELECT * FROM (SELECT ID FROM TABLE)";
                var result = Dialect.GetCountSql(sql);
                Assert.AreEqual($"SELECT COUNT(*) AS {Dialect.OpenQuote}Total{Dialect.CloseQuote} FROM {sql} AS {Dialect.OpenQuote}Total{Dialect.CloseQuote}", result);
            }
        }
    }
}
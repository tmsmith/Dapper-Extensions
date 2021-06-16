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
    public static class SqlServerDialectFixture
    {
        public abstract class SqlServerDialectFixtureBase
        {
            protected SqlServerDialect Dialect;

            [SetUp]
            public void Setup()
            {
                Dialect = new SqlServerDialect();
            }
        }

        [TestFixture]
        public class DatabaseFunctions : SqlServerDialectFixtureBase
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
        public class Properties : SqlServerDialectFixtureBase
        {
            [Test]
            public void CheckSettings()
            {
                Assert.AreEqual('[', Dialect.OpenQuote);
                Assert.AreEqual(']', Dialect.CloseQuote);
                Assert.AreEqual(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.AreEqual('@', Dialect.ParameterPrefix);
                Assert.IsTrue(Dialect.SupportsMultipleStatements);
            }
        }

        [TestFixture]
        public class GetPagingSqlMethod : SqlServerDialectFixtureBase
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
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql("SELECT [schema].[column] FROM [schema].[table]", 0, 10, null, ""));
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
                const string sql = "SELECT [column] FROM [schema].[table] ORDER BY CURRENT_TIMESTAMP OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY";
                var result = Dialect.GetPagingSql("SELECT [column] FROM [schema].[table]", 0, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(parameters["@skipRows"], 0);
                Assert.AreEqual(parameters["@maxResults"], 10);
            }

            [Test]
            public void SelectDistinct_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT DISTINCT [column] FROM [schema].[table] ORDER BY CURRENT_TIMESTAMP OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY";
                var result = Dialect.GetPagingSql("SELECT DISTINCT [column] FROM [schema].[table]", 0, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(parameters["@skipRows"], 0);
            }

            [Test]
            public void SelectOrderBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT [column] FROM [schema].[table] ORDER BY [column] DESC OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY";
                var result = Dialect.GetPagingSql("SELECT [column] FROM [schema].[table] ORDER BY [column] DESC", 0, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(parameters["@skipRows"], 0);
            }
        }

        [TestFixture]
        public class GetOrderByClauseMethod : SqlServerDialectFixtureBase
        {
            [Test]
            public void NoOrderBy_Returns()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table");
                Assert.IsNull(result);
            }

            [Test]
            public void OrderBy_ReturnsItemsAfterClause()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table ORDER BY Column1 ASC, Column2 DESC");
                Assert.AreEqual("ORDER BY Column1 ASC, Column2 DESC", result);
            }

            [Test]
            public void OrderByWithWhere_ReturnsOnlyOrderBy()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table ORDER BY Column1 ASC, Column2 DESC WHERE Column1 = 'value'");
                Assert.AreEqual("ORDER BY Column1 ASC, Column2 DESC", result);
            }
        }

        [TestFixture]
        public class GetIdentitySqlMethod : SqlServerDialectFixtureBase
        {
            private const string _identitySql = "SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [Id]";

            [Test]
            public void NullTableName_ReturnsSql()
            {
                var result = Dialect.GetIdentitySql(null);
                result.Should().Be(_identitySql);
            }

            [Test]
            public void WithTableName_ReturnsSql()
            {
                var result = Dialect.GetIdentitySql("FooTable");
                result.Should().Be(_identitySql);
            }
        }
    }
}
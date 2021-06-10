using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class DB2DialectFixture
    {
        public abstract class DB2DialectFixtureBase
        {
            protected DB2Dialect Dialect;

            [SetUp]
            public void Setup()
            {
                Dialect = new DB2Dialect();
            }
        }

        [TestFixture]
        public class DatabaseFunctions : DB2DialectFixtureBase
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
        public class Properties : DB2DialectFixtureBase
        {
            [Test]
            public void CheckSettings()
            {
                Assert.AreEqual('"', Dialect.OpenQuote);
                Assert.AreEqual('"', Dialect.CloseQuote);
                Assert.AreEqual(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.AreEqual('@', Dialect.ParameterPrefix);
                Assert.IsFalse(Dialect.SupportsMultipleStatements);
            }
        }

        [TestFixture]
        public class GetPagingSqlMethod : DB2DialectFixtureBase
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
                const string sql = "SELECT \"_TEMP\".\"COLUMN\" FROM (SELECT ROW_NUMBER() OVER(ORDER BY CURRENT_TIMESTAMP) AS \"_ROW_NUMBER\", \"COLUMN\" FROM \"SCHEMA\".\"TABLE\") AS \"_TEMP\" WHERE \"_TEMP\".\"_ROW_NUMBER\" BETWEEN @_pageStartRow AND @_pageEndRow";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(parameters["@_pageStartRow"], 1);
                Assert.AreEqual(parameters["@_pageEndRow"], 10);
            }

            [Test]
            public void SelectDistinct_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT \"_TEMP\".\"COLUMN\" FROM (SELECT DISTINCT ROW_NUMBER() OVER(ORDER BY CURRENT_TIMESTAMP) AS \"_ROW_NUMBER\", \"COLUMN\" FROM \"SCHEMA\".\"TABLE\") AS \"_TEMP\" WHERE \"_TEMP\".\"_ROW_NUMBER\" BETWEEN @_pageStartRow AND @_pageEndRow";
                var result = Dialect.GetPagingSql("SELECT DISTINCT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\"", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(parameters["@_pageStartRow"], 1);
                Assert.AreEqual(parameters["@_pageEndRow"], 10);
            }

            [Test]
            public void SelectOrderBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT \"_TEMP\".\"COLUMN\" FROM (SELECT ROW_NUMBER() OVER(ORDER BY \"COLUMN\" DESC) AS \"_ROW_NUMBER\", \"COLUMN\" FROM \"SCHEMA\".\"TABLE\") AS \"_TEMP\" WHERE \"_TEMP\".\"_ROW_NUMBER\" BETWEEN @_pageStartRow AND @_pageEndRow";
                var result = Dialect.GetPagingSql("SELECT \"COLUMN\" FROM \"SCHEMA\".\"TABLE\" ORDER BY \"COLUMN\" DESC", 1, 10, parameters, "");
                Assert.AreEqual(sql, result);
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(parameters["@_pageStartRow"], 1);
                Assert.AreEqual(parameters["@_pageEndRow"], 10);
            }
        }

        [TestFixture]
        public class GetOrderByClauseMethod : DB2DialectFixtureBase
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
        public class GetIdentitySqlMethod : DB2DialectFixtureBase
        {
            private const string _identitySql = "SELECT CAST(IDENTITY_VAL_LOCAL() AS BIGINT) AS \"ID\" FROM SYSIBM.SYSDUMMY1";

            [Test]
            public void NullTableName_ReturnsSql()
            {
                var result = Dialect.GetIdentitySql(null);
                Assert.AreEqual(_identitySql, result);
            }

            [Test]
            public void WithTableName_ReturnsSql()
            {
                var result = Dialect.GetIdentitySql("FooTable");
                Assert.AreEqual(_identitySql, result);
            }
        }

        [TestFixture]
        public class GetColumnNamesMethod : DB2DialectFixtureBase
        {
            [Test]
            public void ColumnNameOnly_ReturnsListOfColumns()
            {
                var result = Dialect.TestProtected().RunMethod<IList<string>>("GetColumnNames", "SELECT Id, Name FROM Table");
                Assert.AreEqual(new List<string> { "Id", "Name" }, result);
            }

            [Test]
            public void PrefixColumnName_ReturnsProperlyQuoted()
            {
                var result = Dialect.TestProtected().RunMethod<IList<string>>("GetColumnNames", "SELECT t.Id, t.Name FROM Table t");
                Assert.AreEqual(new List<string> { "Id", "Name" }, result);
            }

            [Test]
            public void ColumnNameWithAlias_ReturnsListOfColumns()
            {
                var result = Dialect.TestProtected().RunMethod<IList<string>>("GetColumnNames", "SELECT Xpto_ID as Id, Xpto_NAME as Name FROM Table");
                Assert.AreEqual(new List<string> { "Id", "Name" }, result);
            }

            [Test]
            public void ColumnNameWithPrefixAndAlias_ReturnsListOfColumns()
            {
                var result = Dialect.TestProtected().RunMethod<IList<string>>("GetColumnNames", "SELECT t.Xpto_ID as Id, t.Xpto_NAME as Name FROM Table t");
                Assert.AreEqual(new List<string> { "Id", "Name" }, result);
            }
        }
    }
}
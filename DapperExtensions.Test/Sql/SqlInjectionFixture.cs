using DapperExtensions.Sql;
using FluentAssertions;
using NUnit.Framework;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class SqlInjectionFixture
    {
        public abstract class SqlInjectionFixtureBase
        {
            protected IDapperExtensionsConfiguration Configuration;

            [SetUp]
            public void Setup()
            {
                Configuration = new DapperExtensionsConfiguration();
            }
        }

        [TestFixture]
        public class SqlInjectionTest : SqlInjectionFixtureBase
        {
            [Test]
            public void NotExisting_WithoutValue_ReturningNull()
            {
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest));

                result.Should().BeNull();
            }

            [Test]
            public void NotExisting_WithValue_ReturningNull()
            {
                var value = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "foo", Sql = "select foo from dual" };
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), value);

                Assert.IsNotNull(result);
                Assert.AreSame(value, result);
                Assert.AreEqual("foo", value.Property);
                Assert.AreEqual("select foo from dual", value.Sql);
                Assert.AreEqual(value.EntityType, result.EntityType);
            }

            [Test]
            public void Existing_WithoutValue_ReturningValue()
            {
                var value = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "foo", Sql = "select foo from dual" };
                _ = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), value);
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), null);

                result.Should().Be(value);
            }

            [Test]
            public void Existing_WithValue_ReturningNull()
            {
                var value = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "foo", Sql = "select foo from dual" };
                _ = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), value);
                var newValue = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "bar", Sql = "select bar from dual" };
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), newValue);

                result.Should().Be(value);
            }
        }
    }
}

using DapperExtensions.Mapper;
using NUnit.Framework;
using System;

namespace DapperExtensions.Test.Mapper
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class PluralizedAutoClassMapperFixture
    {
        [TestFixture]
        public class PluralizedAutoClassMapperTableName
        {
            [Test]
            public void ReturnsProperPluralization()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("robot");
                Assert.AreEqual("robots", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordEndsWithY()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("penny");
                Assert.AreEqual("pennies", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordEndsWithS()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("mess");
                Assert.AreEqual("messes", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordEndsWithF()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("life");
                Assert.AreEqual("lives", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordWithFe()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("leaf");
                Assert.AreEqual("leaves", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordContainsF()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("profile");
                Assert.AreEqual("profiles", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordContainsFe()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("effect");
                Assert.AreEqual("effects", m.TableName);
            }

            private static PluralizedAutoClassMapper<T> GetMapper<T>() where T : class
            {
                return new PluralizedAutoClassMapper<T>();
            }
        }

        [TestFixture]
        public class CustomPluralizedMapperTableName
        {
            [Test]
            public void ReturnsProperPluralization()
            {
                CustomPluralizedMapper<Foo> m = GetMapper<Foo>();
                m.Table("Dog");
                Assert.AreEqual("Dogs", m.TableName);
            }

            [Test]
            public void ReturnsProperResultsForExceptions()
            {
                CustomPluralizedMapper<Foo> m = GetMapper<Foo>();
                m.Table("Person");
                Assert.AreEqual("People", m.TableName);
            }

            private static CustomPluralizedMapper<T> GetMapper<T>() where T : class
            {
                return new CustomPluralizedMapper<T>();
            }

            public class CustomPluralizedMapper<T> : PluralizedAutoClassMapper<T> where T : class
            {
                public override void Table(string tableName)
                {
                    if (tableName.Equals("Person", StringComparison.CurrentCultureIgnoreCase))
                    {
                        TableName = "People";
                    }
                    else
                    {
                        base.Table(tableName);
                    }
                }
            }
        }

        private class Foo
        {
        }
    }
}

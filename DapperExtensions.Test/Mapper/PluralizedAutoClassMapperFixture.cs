using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;
using NUnit.Framework;

namespace DapperExtensions.Test.Mapper
{
    [TestFixture]
    public class PluralizedAutoClassMapperFixture
    {
        [TestFixture]
        public class PluralizedAutoClassMapperTableName
        {
            [Test]
            public void ReturnsProperPluralization()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("robot");
                Assert.AreEqual("robots", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordEndsWithY()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("penny");
                Assert.AreEqual("pennies", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordEndsWithS()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("mess");
                Assert.AreEqual("messes", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordEndsWithF()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("life");
                Assert.AreEqual("lives", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordWithFe()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("leaf");
                Assert.AreEqual("leaves", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordContainsF()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("profile");
                Assert.AreEqual("profiles", m.TableName);
            }

            [Test]
            public void ReturnsProperPluralizationWhenWordContainsFe()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("effect");
                Assert.AreEqual("effects", m.TableName);
            }

            private PluralizedAutoClassMapper<T> GetMapper<T>() where T : class
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
                m.SetTableName("Dog");
                Assert.AreEqual("Dogs", m.TableName);
            }

            [Test]
            public void ReturnsProperResultsForExceptions()
            {
                CustomPluralizedMapper<Foo> m = GetMapper<Foo>();
                m.SetTableName("Person");
                Assert.AreEqual("People", m.TableName);
            }

            private CustomPluralizedMapper<T> GetMapper<T>() where T : class
            {
                return new CustomPluralizedMapper<T>();
            }

            public class CustomPluralizedMapper<T> : PluralizedAutoClassMapper<T> where T : class
            {
                public override void SetTableName(string tableName)
                {
                    if (tableName.Equals("Person", StringComparison.CurrentCultureIgnoreCase))
                    {
                        TableName = "People";
                    }
                    else
                    {
                        base.SetTableName(tableName);
                    }
                }
            }
        }

        private class Foo
        {
        }
    }
}

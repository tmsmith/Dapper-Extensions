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

            private PluralizedAutoClassMapper<T> GetMapper<T>() where T : class
            {
                return new PluralizedAutoClassMapper<T>();
            }
        }

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

            private CustomPluralizedMapper<T> GetMapper<T>() where T : class
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

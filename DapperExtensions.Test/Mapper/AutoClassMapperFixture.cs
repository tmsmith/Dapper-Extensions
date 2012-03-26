using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;
using NUnit.Framework;

namespace DapperExtensions.Test.Mapper
{
    [TestFixture]
    public class AutoClassMapperFixture
    {
        [TestFixture]
        public class AutoClassMapperTableName
        {
            [Test]
            public void Constructor_ReturnsProperName()
            {
                AutoClassMapper<Foo> m = GetMapper<Foo>();
                Assert.AreEqual("Foo", m.TableName);
            }

            [Test]
            public void SettingTableName_ReturnsProperName()
            {
                AutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("Barz");
                Assert.AreEqual("Barz", m.TableName);
            }

            private AutoClassMapper<T> GetMapper<T>() where T : class
            {
                return new AutoClassMapper<T>();
            }
        }

        [TestFixture]
        public class CustomAutoMapperTableName
        {
            [Test]
            public void ReturnsProperPluralization()
            {
                CustomAutoMapper<Foo> m = GetMapper<Foo>();
                Assert.AreEqual("Foo", m.TableName);
            }

            [Test]
            public void ReturnsProperResultsForExceptions()
            {
                CustomAutoMapper<Foo2> m = GetMapper<Foo2>();
                Assert.AreEqual("TheFoo", m.TableName);
            }

            private CustomAutoMapper<T> GetMapper<T>() where T : class
            {
                return new CustomAutoMapper<T>();
            }

            public class CustomAutoMapper<T> : AutoClassMapper<T> where T : class
            {
                public override void Table(string tableName)
                {
                    if (tableName.Equals("Foo2", StringComparison.CurrentCultureIgnoreCase))
                    {
                        TableName = "TheFoo";
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

        private class Foo2
        {
        }
    }
}

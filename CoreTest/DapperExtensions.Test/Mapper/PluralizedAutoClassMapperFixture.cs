using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;
using Xunit;

namespace DapperExtensions.Test.Mapper
{
   
    public class PluralizedAutoClassMapperFixture
    {
       
        public class PluralizedAutoClassMapperTableName
        {
            [Fact]
            public void ReturnsProperPluralization()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("robot");
                Assert.Equal("robots", m.TableName);
            }

            [Fact]
            public void ReturnsProperPluralizationWhenWordEndsWithY()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("penny");
                Assert.Equal("pennies", m.TableName);
            }

            [Fact]
            public void ReturnsProperPluralizationWhenWordEndsWithS()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("mess");
                Assert.Equal("messes", m.TableName);
            }

            [Fact]
            public void ReturnsProperPluralizationWhenWordEndsWithF()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("life");
                Assert.Equal("lives", m.TableName);
            }

            [Fact]
            public void ReturnsProperPluralizationWhenWordWithFe()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("leaf");
                Assert.Equal("leaves", m.TableName);
            }

            [Fact]
            public void ReturnsProperPluralizationWhenWordContainsF()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("profile");
                Assert.Equal("profiles", m.TableName);
            }

            [Fact]
            public void ReturnsProperPluralizationWhenWordContainsFe()
            {
                PluralizedAutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("effect");
                Assert.Equal("effects", m.TableName);
            }

            private PluralizedAutoClassMapper<T> GetMapper<T>() where T : class
            {
                return new PluralizedAutoClassMapper<T>();
            }
        }

       
        public class CustomPluralizedMapperTableName
        {
            [Fact]
            public void ReturnsProperPluralization()
            {
                CustomPluralizedMapper<Foo> m = GetMapper<Foo>();
                m.Table("Dog");
                Assert.Equal("Dogs", m.TableName);
            }

            [Fact]
            public void ReturnsProperResultsForExceptions()
            {
                CustomPluralizedMapper<Foo> m = GetMapper<Foo>();
                m.Table("Person");
                Assert.Equal("People", m.TableName);
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

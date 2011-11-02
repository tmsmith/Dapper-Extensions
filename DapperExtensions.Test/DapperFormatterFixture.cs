using System;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    public class DapperFormatterFixture : BaseFixture
    {
        private class Foo
        {
            public int Bar { get; set; }
            public string Baz { get; set; }
            public string Email { get; set; }
        }

        private class CustomMapper : ClassMapper<Foo>
        {
            public CustomMapper()
            {
                this.Map(f => f.Bar).Column("Bar");
                this.Map(f => f.Email).Column("EmailAddress");
            }
        }

        [Test]
        public void GetTableName_Returns_Properly_Formatted_Name_When_Schema_Provided()
        {
            DefaultFormatter defaultFormatter = new DefaultFormatter();
            ClassMapper<Foo> mapper = new ClassMapper<Foo>();
            mapper.Schema("clients");
            var tableName = defaultFormatter.GetTableName(mapper);
            Assert.AreEqual("[clients].[Foo]", tableName);
        }

        [Test]
        public void GetTableName_Returns_Properly_Formatted_Name_Without_Schema()
        {
            DefaultFormatter defaultFormatter = new DefaultFormatter();
            ClassMapper<Foo> mapper = new ClassMapper<Foo>();
            var tableName = defaultFormatter.GetTableName(mapper);
            Assert.AreEqual("[Foo]", tableName);
        }

        [Test]
        public void GetColumnName_Returns_Aliased_Column_When_Mapped_And_IncludeAlias_Is_True()
        {
            DefaultFormatter defaultFormatter = new DefaultFormatter();
            CustomMapper mapper = new CustomMapper();

            var columnName = defaultFormatter.GetColumnName(mapper, mapper.Properties[1], true);
            Assert.AreEqual("[Foo].[EmailAddress] AS [Email]", columnName);
        }

        [Test]
        public void GetColumnName_Returns_Table_Column_Name_When_Mapped_And_IncludeAlias_Is_False()
        {
            DefaultFormatter defaultFormatter = new DefaultFormatter();
            CustomMapper mapper = new CustomMapper();

            var columnName = defaultFormatter.GetColumnName(mapper, mapper.Properties[1], false);
            Assert.AreEqual("[Foo].[EmailAddress]", columnName);
        }

        [Test]
        public void GetColumnName_Throws_Exception_When_Invalid_Column_Name_Provided()
        {
            DefaultFormatter defaultFormatter = new DefaultFormatter();
            CustomMapper mapper = new CustomMapper();

                Assert.Throws<ArgumentException>(() => defaultFormatter.GetColumnName(mapper, "Something", false),
                              "Could not find 'Something' in Mapping.");
        }

        [Test]
        public void GetColumnName_Returns_Correct_Name_When_PropertyName_Provided()
        {
            DefaultFormatter defaultFormatter = new DefaultFormatter();
            CustomMapper mapper = new CustomMapper();

            var name = defaultFormatter.GetColumnName(mapper, "Bar", false);
            Assert.AreEqual("[Foo].[Bar]", name);
        }

        [Test]
        public void GetNextGuid_Returns_Guid()
        {
            DefaultFormatter defaultFormatter = new DefaultFormatter();
            defaultFormatter.GetNextGuid();
        }
    }
}
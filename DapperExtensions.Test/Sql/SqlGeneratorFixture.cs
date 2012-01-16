using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Mapper;
using NUnit.Framework;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    public class SqlGeneratorFixture
    {
        [Test]
        public void GetTableName_Returns_Properly_Formatted_Name_When_Schema_Provided()
        {
            ClassMapper<Foo> mapper = new ClassMapper<Foo>();
            mapper.Schema("clients");
            var tableName = DapperExtensions.SqlGenerator.GetTableName(mapper);
            Assert.AreEqual("[clients].[Foo]", tableName);
        }

        [Test]
        public void GetTableName_Returns_Properly_Formatted_Name_Without_Schema()
        {
            ClassMapper<Foo> mapper = new ClassMapper<Foo>();
            var tableName = DapperExtensions.SqlGenerator.GetTableName(mapper);
            Assert.AreEqual("[Foo]", tableName);
        }

        [Test]
        public void GetColumnName_Returns_Aliased_Column_When_Mapped_And_IncludeAlias_Is_True()
        {
            CustomMapper mapper = new CustomMapper();
            var columnName = DapperExtensions.SqlGenerator.GetColumnName(mapper, mapper.Properties[1], true);
            Assert.AreEqual("[Foo].[EmailAddress] AS [Email]", columnName);
        }

        [Test]
        public void GetColumnName_Returns_Table_Column_Name_When_Mapped_And_IncludeAlias_Is_False()
        {
            CustomMapper mapper = new CustomMapper();
            var columnName = DapperExtensions.SqlGenerator.GetColumnName(mapper, mapper.Properties[1], false);
            Assert.AreEqual("[Foo].[EmailAddress]", columnName);
        }

        [Test]
        public void GetColumnName_Throws_Exception_When_Invalid_Column_Name_Provided()
        {
            CustomMapper mapper = new CustomMapper();
            Assert.Throws<ArgumentException>(() => DapperExtensions.SqlGenerator.GetColumnName(mapper, "Something", false),
                          "Could not find 'Something' in Mapping.");
        }

        [Test]
        public void GetColumnName_Returns_Correct_Name_When_PropertyName_Provided()
        {
            CustomMapper mapper = new CustomMapper();
            var name = DapperExtensions.SqlGenerator.GetColumnName(mapper, "Bar", false);
            Assert.AreEqual("[Foo].[Bar]", name);
        }

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
                Map(f => f.Bar).Column("Bar");
                Map(f => f.Email).Column("EmailAddress");
            }
        }
    }
}
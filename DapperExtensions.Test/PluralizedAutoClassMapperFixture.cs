using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class PluralizedAutoClassMapperFixture : BaseFixture
    {
        [Test]
        public void TableName_Should_Properly_Pluralize()
        {
            PluralizedAutoClassMapper<Foo> m = new PluralizedAutoClassMapper<Foo>();
            m.Table("robot");
            Assert.AreEqual("robots", m.TableName);
        }

        [Test]
        public void TableName_Should_Properly_Pluralize_Words_Ending_With_Y()
        {
            PluralizedAutoClassMapper<Foo> m = new PluralizedAutoClassMapper<Foo>();
            m.Table("penny");
            Assert.AreEqual("pennies", m.TableName);
        }

        [Test]
        public void TableName_Should_Properly_Pluralize_Words_Ending_With_S()
        {
            PluralizedAutoClassMapper<Foo> m = new PluralizedAutoClassMapper<Foo>();
            m.Table("mess");
            Assert.AreEqual("messes", m.TableName);
        }

        [Test]
        public void Custom_Pluralized_Mapper_Should_Process_Exceptions()
        {
            CustomPluralizedMapper<Foo> m = new CustomPluralizedMapper<Foo>();
            m.Table("Person");
            Assert.AreEqual("Persons", m.TableName);
        }

        [Test]
        public void Custom_Pluralized_Mapper_Should_Process_Singluar_Names()
        {
            CustomPluralizedMapper<Foo> m = new CustomPluralizedMapper<Foo>();
            m.Table("Dog");
            Assert.AreEqual("Dogs", m.TableName);
        }

        public class Foo
        {
        }

        public class CustomPluralizedMapper<T> : PluralizedAutoClassMapper<T> where T : class 
        {
            public override void Table(string tableName)
            {
                if (tableName.Equals("Person", StringComparison.CurrentCultureIgnoreCase))
                {
                    TableName = "Persons";
                }

                base.Table(tableName);
            }
        }
    }
}

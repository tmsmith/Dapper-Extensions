using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Test.Data;
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

        public class Foo
        {
        }
    }
}

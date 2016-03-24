using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test
{
    [TestFixture]
    public class ReflectionHelperFixture
    {
        private class Foo
        {
            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        [Test]
        public void GetProperty_Returns_MemberInfo_For_Correct_Property()
        {
            Expression<Func<Foo, object>> expression = f => f.Bar;
            var m = ReflectionHelper.GetProperty(expression);
            Assert.AreEqual("Bar", m.Name);
        }

        [Test]
        public void GetObjectValues_Returns_Dictionary_With_Property_Value_Pairs()
        {
            Foo f = new Foo { Bar = 3, Baz = "Yum" };

            var dictionary = ReflectionHelper.GetObjectValues(f, null);
            Assert.AreEqual(3, dictionary["Bar"]);
            Assert.AreEqual("Yum", dictionary["Baz"]);
        }

        [Test]
        public void GetObjectValues_Returns_Empty_Dictionary_When_Null_Object_Provided()
        {
            var dictionary = ReflectionHelper.GetObjectValues(null, null);
            Assert.AreEqual(0, dictionary.Count);
        }
    }
}
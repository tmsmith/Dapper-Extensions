using System;
using System.Linq.Expressions;
using DapperExtensions;
using Xunit;

namespace DapperExtensions.Test
{
    public class ReflectionHelperFixture
    {
        private class Foo
        {
            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        [Fact]
        public void GetProperty_Returns_MemberInfo_For_Correct_Property()
        {
            Expression<Func<Foo, object>> expression = f => f.Bar;
            var m = ReflectionHelper.GetProperty(expression);
            Assert.Equal("Bar", m.Name);
        }

        [Fact]
        public void GetObjectValues_Returns_Dictionary_With_Property_Value_Pairs()
        {
            Foo f = new Foo { Bar = 3, Baz = "Yum" };

            var dictionary = ReflectionHelper.GetObjectValues(f);
            Assert.Equal(3, dictionary["Bar"]);
            Assert.Equal("Yum", dictionary["Baz"]);
        }

        [Fact]
        public void GetObjectValues_Returns_Empty_Dictionary_When_Null_Object_Provided()
        {
            var dictionary = ReflectionHelper.GetObjectValues(null);
            Assert.Equal(0, dictionary.Count);
        }
    }
}
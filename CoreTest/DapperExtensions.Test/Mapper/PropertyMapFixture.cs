using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DapperExtensions.Mapper;
using Xunit;

namespace DapperExtensions.Test.Mapper
{
    public class PropertyMapFixture
    {
        private class Foo
        {
            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        [Fact]
        public void PropertyMap_Constructor_Sets_Name_And_ColumnName_Property_From_PropertyInfo()
        {
            Expression<Func<Foo, object>> expression = f => f.Bar;
            var pi = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            PropertyMap pm = new PropertyMap(pi);
            Assert.Equal("Bar", pm.Name);
            Assert.Equal("Bar", pm.ColumnName);
        }

        [Fact]
        public void PropertyMap_Column_Sets_ColumnName_But_Does_Not_Change_Name()
        {
            Expression<Func<Foo, object>> expression = f => f.Baz;
            var pi = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            PropertyMap pm = new PropertyMap(pi);
            pm.Column("X");
            Assert.Equal("Baz", pm.Name);
            Assert.Equal("X", pm.ColumnName);
        }

        [Fact]
        public void PropertyMap_Key_Sets_KeyType()
        {
            Expression<Func<Foo, object>> expression = f => f.Baz;
            var pi = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            PropertyMap pm = new PropertyMap(pi);
            pm.Column("X").Key(KeyType.Identity);
            Assert.Equal("Baz", pm.Name);
            Assert.Equal("X", pm.ColumnName);
            Assert.Equal(KeyType.Identity, pm.KeyType);
        }
    }
}
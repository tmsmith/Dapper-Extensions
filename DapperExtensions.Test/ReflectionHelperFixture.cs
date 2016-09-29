using System;
using System.Linq.Expressions;
using DapperExtensions.Mapper;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class ReflectionHelperFixture
    {
        private class Foo
        {
            public int Bar { get; set; }
            public string Baz { get; set; }
		}

		public class BaseRecord
		{
			public Guid Id { get; set; }

			public class Map : ClassMapper<BaseRecord>
			{
				public Map()
				{
					Map(x => x.Id).Key(KeyType.Generated);
				}
			}
		}
		public class InheritingRecord : BaseRecord
		{
			public new Guid Id { get; set; }
			public Guid BaseRecordId
			{
				get { return base.Id; }
				set { base.Id = value; }
			}

			public class Map2 : ClassMapper<InheritingRecord>
			{
				public Map2()
				{
					Map(x => x.Id).Key(KeyType.Generated);
					Map(x => x.BaseRecordId).Key(KeyType.Assigned);
					AutoMap((t, pi) => false);
				}
			}
		}

		public abstract class BaseRecordWithAbstract
		{
			public abstract Guid Id { get; set; }

			public class Map : ClassMapper<BaseRecordWithAbstract>
			{
				public Map()
				{
					Map(x => x.Id).Key(KeyType.Generated);
				}
			}
		}
		public class OverridingRecord : BaseRecordWithAbstract
		{
			public override Guid Id { get; set; }

			public class Map2 : ClassMapper<OverridingRecord>
			{
				public Map2()
				{
					Map(x => x.Id).Key(KeyType.Generated);
					AutoMap((t, pi) => false);
				}
			}
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

            var dictionary = ReflectionHelper.GetObjectValues(f);
            Assert.AreEqual(3, dictionary["Bar"]);
            Assert.AreEqual("Yum", dictionary["Baz"]);
        }

        [Test]
        public void GetObjectValues_Returns_Empty_Dictionary_When_Null_Object_Provided()
        {
            var dictionary = ReflectionHelper.GetObjectValues(null);
            Assert.AreEqual(0, dictionary.Count);
        }

		[Test]
		public void GetObjectValuesReturnsValuesForCorrectType()
		{
			var record = new InheritingRecord
			{
				Id = Guid.NewGuid(),
				BaseRecordId = Guid.NewGuid()
			};

			var dictionary = ReflectionHelper.GetObjectValues((BaseRecord)record);
			Assert.AreNotEqual((record as BaseRecord).Id, dictionary["Id"]);
			Assert.AreEqual(record.Id, dictionary["Id"]);//--Really shouldn't be equal (old functionality)

			var baseDictionary = ReflectionHelper.GetTypeValues((BaseRecord)record);
			Assert.AreEqual((record as BaseRecord).Id, baseDictionary[typeof(BaseRecord).GetProperty("Id")]);
			Assert.AreNotEqual(record.Id, baseDictionary[typeof(BaseRecord).GetProperty("Id")]);
			Assert.IsFalse(baseDictionary.ContainsKey(typeof(InheritingRecord).GetProperty("Id")));

			var inheritingDictionary = ReflectionHelper.GetTypeValues(record);
			Assert.IsFalse(inheritingDictionary.ContainsKey(typeof(BaseRecord).GetProperty("Id")));
			Assert.AreEqual(record.Id, inheritingDictionary[typeof(InheritingRecord).GetProperty("Id")]);
			Assert.AreEqual(record.BaseRecordId, inheritingDictionary[typeof(InheritingRecord).GetProperty("BaseRecordId")]);
		}

		[Test]
		public void GetObjectValuesWorksWithOverriddenProperty()
		{
			var record = new OverridingRecord
			{
				Id = Guid.NewGuid()
			};

			var dictionary = ReflectionHelper.GetObjectValues((BaseRecordWithAbstract)record);
			Assert.AreEqual((record as BaseRecordWithAbstract).Id, dictionary["Id"]);
			Assert.AreEqual(record.Id, dictionary["Id"]);

			var baseDictionary = ReflectionHelper.GetTypeValues((BaseRecordWithAbstract)record);
			Assert.AreEqual((record as BaseRecordWithAbstract).Id, baseDictionary[typeof(BaseRecordWithAbstract).GetProperty("Id")]);
			Assert.AreEqual(record.Id, baseDictionary[typeof(BaseRecordWithAbstract).GetProperty("Id")]);
			Assert.IsFalse(baseDictionary.ContainsKey(typeof(OverridingRecord).GetProperty("Id")));

			var inheritingDictionary = ReflectionHelper.GetTypeValues(record);
			Assert.IsFalse(inheritingDictionary.ContainsKey(typeof(BaseRecordWithAbstract).GetProperty("Id")));
			Assert.AreEqual(record.Id, inheritingDictionary[typeof(OverridingRecord).GetProperty("Id")]);
		}
	}
}
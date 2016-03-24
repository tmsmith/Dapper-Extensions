using System;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures
{
    public abstract partial class FixturesBase
    {
        [Test]
        public void UsingKey_ReturnsEntity()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            int id = personRepository.Insert(p1);

            Person p2 = personRepository.Get(id);
            Assert.AreEqual(id, p2.Id);
            Assert.AreEqual("Foo", p2.FirstName);
            Assert.AreEqual("Bar", p2.LastName);
        }

        [Test]
        public void UsingCompositeKey_ReturnsEntity()
        {
            var multiKeyRepository = Container.Resolve<IRepository<Multikey>>();

            var m1 = new Multikey { Key2 = "key", Value = "bar" };
            dynamic key = multiKeyRepository.Insert(m1);
            int key1 = key.Key1;
            string key2 = key.Key2;

            Multikey m2 = multiKeyRepository
                .Query(e => e.Key1 == key1 && e.Key2 == key2)
                .Single();

            Assert.AreEqual(key.Key1, m2.Key1);
            Assert.AreEqual("key", m2.Key2);
            Assert.AreEqual("bar", m2.Value);
        }

        [Test]
        public void UsingCustomTableAndPropertyNames_ReturnsEntity()
        {
            var phoneRepository = Container.Resolve<IRepository<Phone>>();

            var phone = new Phone { Value = "0800123456" };
            int id = phoneRepository.Insert(phone);

            Phone phone2 = phoneRepository.Get(id);

            Assert.AreEqual(id, phone2.Id);
            Assert.AreEqual(phone.Value, phone2.Value);
        }
    }
}
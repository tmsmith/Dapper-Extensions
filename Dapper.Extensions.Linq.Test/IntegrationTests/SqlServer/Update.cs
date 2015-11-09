using System;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Update : SqlServerBase
    {
        [Test]
        public void UsingKey_UpdatesEntity()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            int id = personRepository.Insert(p1);
            var p2 = personRepository.Get(id);
            p2.FirstName = "Baz";
            p2.Active = false;

            personRepository.Update(p2);

            var p3 = personRepository.Get(id);
            Assert.AreEqual("Baz", p3.FirstName);
            Assert.AreEqual("Bar", p3.LastName);
            Assert.AreEqual(false, p3.Active);
        }

        [Test]
        public void UsingCompositeKey_UpdatesEntity()
        {

            var multiKeyRepository = Container.Resolve<IRepository<Multikey>>();

            var m = new Multikey { Key2 = "key", Value = "foo" };
            var key = multiKeyRepository.Insert(m);
            Assert.AreEqual(1, key.Key1);
            Assert.AreEqual("key", key.Key2);

            int key1 = key.Key1;
            string key2 = key.Key2;

            Multikey m2 = multiKeyRepository
                .Query(e => e.Key1 == key1 && e.Key2 == key2)
                .Single();
            m2.Key2 = "key";
            m2.Value = "barz";
            multiKeyRepository.Update(m2);

            Multikey m3 = multiKeyRepository
                .Query(e => e.Key1 == key1 && e.Key2 == key2)
                .Single();

            Assert.AreEqual(1, m3.Key1);
            Assert.AreEqual("key", m3.Key2);
            Assert.AreEqual("barz", m3.Value);
        }
    }
}

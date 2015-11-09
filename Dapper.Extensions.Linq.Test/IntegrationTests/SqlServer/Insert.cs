using System;
using System.Data.Entity;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Insert : SqlServerBase
    {
        [Test]
        public void AddsEntityTo_database_ReturnsKey()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            Person p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            int id = personRepository.Insert(p);
            Assert.AreEqual(p.Id, id);
        }

        [Test]
        public void AddsEntityTo_database_ReturnsCompositeKey()
        {
            var multiKeyRepository = Container.Resolve<IRepository<Multikey>>();

            var m = new Multikey { Key2 = "key", Value = "foo" };
            var key = multiKeyRepository.Insert(m);
            Assert.AreEqual(1, key.Key1);
            Assert.AreEqual("key", key.Key2);
        }

        [Test]
        public void AddsEntityTo_database_ReturnsGeneratedPrimaryKey()
        {
            var animalRepository = Container.Resolve<IRepository<Animal>>();

            var a1 = new Animal { Name = "Foo" };
            Guid id = animalRepository.Insert(a1);

            Assert.AreNotEqual(Guid.Empty, id);
            Assert.AreEqual(a1.Id, id);
        }
    }
}
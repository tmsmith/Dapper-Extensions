using System;
using System.Linq;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Count : SqlServerBase
    {
        [Test]
        public void Returns_Count_All()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

            int count = personRepository.GetList().Count;
            Assert.AreEqual(4, count);
        }

        [Test]
        public void Returns_Count_Filtered()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });
            var filterBy = new[] { "a", "b" };

            int count = personRepository.Query(e => filterBy.Contains(e.FirstName)).Count();
            Assert.AreEqual(2, count);
        }
    }
}
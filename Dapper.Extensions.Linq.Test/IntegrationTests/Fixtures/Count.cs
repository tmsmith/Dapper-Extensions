using System;
using System.Linq;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures
{
    public abstract partial class FixturesBase
    {

        [Test]
        public void Returns_Count_All()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            int countList = personRepository.GetList().Count;
            int count = personRepository.Count();

            Assert.AreEqual(countList, count);
        }

        [Test]
        public void Returns_Count_Filtered()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });
            var filterBy = new[] { "a" };

            int count = personRepository.Query(e => filterBy.Contains(e.FirstName)).Count();
            Assert.Greater(count, 0);
        }
    }
}
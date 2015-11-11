using System;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures
{
    public abstract partial class FixturesBase
    {
        [Test]
        public void UsingGetList_ReturnsAll()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            int listCount = personRepository.GetList().Count;
            int count = personRepository.Count();

            Assert.AreEqual(listCount, count);  
        }
    }
}
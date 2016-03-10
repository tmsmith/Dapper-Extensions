using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class QueryDynamic : SqlServerOnly
    {
        [Test]
        public void UsingQuery_Dynamic()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            var person = new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow };
            personRepository.Insert(person);

            const string sql = "SELECT TOP 1 Id, FirstName FROM Person ORDER BY Id DESC";
            List<dynamic> result = personRepository.QueryDynamic(sql).ToList();

            Assert.AreEqual(result.First().Id, person.Id);
            Assert.AreEqual(result.First().FirstName, person.FirstName);
        }
    }
}
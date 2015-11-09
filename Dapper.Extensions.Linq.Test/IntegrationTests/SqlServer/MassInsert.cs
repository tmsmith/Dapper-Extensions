using System;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class MassInsert : SqlServerBase
    {
        private const int Count = 1000;

        [Test]
        public void IdentityKey_UsingEntity()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            DateTime start = DateTime.Now;
            for (int i = 0; i < Count; i++)
            {
                Person p2 = new Person { FirstName = "FirstName" + i, LastName = "LastName" + i, DateCreated = DateTime.Now, Active = true };
                personRepository.Insert(p2);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void GuidKey_UsingEntity()
        {
            var animalRepository = Container.Resolve<IRepository<Animal>>();

            DateTime start = DateTime.Now;
            for (int i = 0; i < Count; i++)
            {
                Animal a2 = new Animal { Name = "Name" + i };
                animalRepository.Insert(a2);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void AssignKey_UsingEntity()
        {
            var carRepository = Container.Resolve<IRepository<Car>>();

            DateTime start = DateTime.Now;
            for (int i = 0; i < Count; i++)
            {
                var key = (i + 1).ToString().PadLeft(15, '0');
                Car ca2 = new Car { Id = key, Name = "Name" + i };
                carRepository.Insert(ca2);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }
    }
}
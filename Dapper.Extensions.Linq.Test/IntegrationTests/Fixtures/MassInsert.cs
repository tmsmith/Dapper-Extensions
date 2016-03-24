using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures
{
    public abstract partial class FixturesBase
    {
        private const int Count = 100;

        [Test]
        public void IdentityKey_UsingEntity()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            DateTime start = DateTime.Now;
            var people = new List<Person>();
            for (int i = 0; i < Count; i++)
            {
                var person = new Person { FirstName = "FirstName" + i, LastName = "LastName" + i, DateCreated = DateTime.Now, Active = true };
                people.Add(person);
            }

            personRepository.Insert(people);

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void GuidKey_UsingEntity()
        {
            var animalRepository = Container.Resolve<IRepository<Animal>>();

            DateTime start = DateTime.Now;
            var animals = new List<Animal>();
            for (int i = 0; i < Count; i++)
            {
                var animal = new Animal { Name = "Name" + i };
                animals.Add(animal);
            }
            animalRepository.Insert(animals);

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void AssignKey_UsingEntity()
        {
            var carRepository = Container.Resolve<IRepository<Car>>();

            DateTime start = DateTime.Now;
            var cars = new List<Car>();
            for (int i = 0; i < Count; i++)
            {
                var key = (i + 1).ToString().PadLeft(15, '0');
                Car car = new Car { Id = key, Name = "Name" + i };
                cars.Add(car);
            }

            carRepository.Insert(cars);
            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }
    }
}
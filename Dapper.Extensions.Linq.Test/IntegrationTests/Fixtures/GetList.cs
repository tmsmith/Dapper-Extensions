﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Extensions;
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

        [Test]
        public void UsingQuery_ReturnsAll()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            int listCount = personRepository.GetList().Count;
            int count = personRepository.Query().Count();


            Assert.AreEqual(listCount, count);
        }

        [Test]
        public void UsingQuery_OrderBy()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            personRepository.Delete();

            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            var person = personRepository.Query().OrderBy(e => e.FirstName).FirstOrDefault();

            StringAssert.AreEqualIgnoringCase(person.FirstName, "a");
        }

        [Test]
        public void UsingQuery_OrderByDescending()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            personRepository.Delete();

            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            var person = personRepository.Query().OrderByDescending(e => e.FirstName).FirstOrDefault();

            StringAssert.AreEqualIgnoringCase(person.FirstName, "d");
        }

        [Test]
        public void UsingQuery_Take()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            personRepository.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            personRepository.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            var topPersonsCount = personRepository.Query().Take(2).Count();

            Assert.AreEqual(2, topPersonsCount);
        }

        [Test]
        public void UsingQuery_List_Contains()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            var person = new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow };
            int id = personRepository.Insert(person);

            var people = new List<int> { id };

            int personCount = personRepository
                .Query(e => people.Contains(e.Id))
                .Count();

            Assert.AreEqual(1, personCount);
        }

        [Test]
        public void UsingQuery_List_Contains_Nullable()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            var person = new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow, ProfileId = 12 };
            personRepository.Insert(person);

            var profileIds = new List<int> { 12 };

            int personCount = personRepository
                .Query(e => profileIds.Contains(e.ProfileId.Value))
                .Count();

            Assert.AreEqual(1, personCount);
        }

        [Test]
        public void UsingQuery_Enumerable_Contains()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            var person = new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow };
            int id = personRepository.Insert(person);

            IEnumerable<int> people = new List<int> { id };

            int personCount = personRepository
                .Query(e => people.Contains(e.Id))
                .Count();

            Assert.AreEqual(1, personCount);
        }

        [Test]
        public void UsingQuery_Enumerable_Contains_Nullable()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();
            var person = new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow, ProfileId = 13 };
            personRepository.Insert(person);

            var profileIds = new List<int> { 13 };

            int personCount = personRepository
                .Query(e => profileIds.Contains(e.ProfileId.Value))
                .Count();

            Assert.AreEqual(1, personCount);
        }

        [Test]
        public void UsingQuery_With_Expression()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            var person = new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow, ProfileId = 14 };
            personRepository.Insert(person);

            Expression<Func<Person, bool>> expression = PredicateBuilder.True<Person>();
            expression = expression.And(e => e.ProfileId == 14);

            int count = personRepository
                .Query(expression)
                .Count();

            Assert.AreEqual(1, count);
        }

        [Test]
        public void UsingQuery_With_Expression_Value_Contains()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            personRepository.Delete();

            var person = new Person { Active = false, FirstName = "ba", LastName = "b1", DateCreated = DateTime.UtcNow, ProfileId = 14 };
            personRepository.Insert(person);

            Expression<Func<Person, bool>> expression = PredicateBuilder.True<Person>();
            expression = expression.And(e => e.FirstName.Contains("b"));

            int count = personRepository
                .Query(expression)
                .Count();

            Assert.AreEqual(1, count);
        }

        [Test]
        public void UsingQuery_With_Expression_Ordered_By_Name_Constant()
        {
            var personRepository = Container.Resolve<IRepository<Person>>();

            personRepository.Delete();

            var person = new Person
            {
                Active = false,
                FirstName = "b",
                LastName = "b1",
                DateCreated = DateTime.UtcNow
            };
            personRepository.Insert(person);

            person = new Person
            {
                Active = false,
                FirstName = "b",
                LastName = "b2",
                DateCreated = DateTime.UtcNow
            };
            personRepository.Insert(person);

            const string firstName = "b";
            Expression<Func<Person, bool>> expression = PredicateBuilder.True<Person>();
            expression = expression.And(e => e.FirstName == firstName);

            int count = personRepository
                .Query(expression)
                .OrderBy(e => e.FirstName)
                .Count();

            Assert.AreEqual(2, count);
        }
    }
}
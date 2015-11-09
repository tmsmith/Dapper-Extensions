using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class MassInsert : SqlServerTests
    {
        private const int Count = 1000;

        [Test]
        public void IdentityKey_UsingEntity()
        {
            Person p = new Person
            {
                FirstName = "FirstName",
                LastName = "LastName",
                DateCreated = DateTime.Now,
                Active = true
            };
            Database.Insert(p);
            DateTime start = DateTime.Now;
            List<int> ids = new List<int>();
            for (int i = 0; i < Count; i++)
            {
                Person p2 = new Person
                {
                    FirstName = "FirstName" + i,
                    LastName = "LastName" + i,
                    DateCreated = DateTime.Now,
                    Active = true
                };
                Database.Insert(p2);
                ids.Add(p2.Id);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void IdentityKey_UsingReturnValue()
        {
            Person p = new Person
            {
                FirstName = "FirstName",
                LastName = "LastName",
                DateCreated = DateTime.Now,
                Active = true
            };
            Database.Insert(p);
            DateTime start = DateTime.Now;
            List<int> ids = new List<int>();
            for (int i = 0; i < Count; i++)
            {
                Person p2 = new Person
                {
                    FirstName = "FirstName" + i,
                    LastName = "LastName" + i,
                    DateCreated = DateTime.Now,
                    Active = true
                };
                var id = Database.Insert(p2);
                ids.Add(id);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void GuidKey_UsingEntity()
        {
            Animal a = new Animal { Name = "Name" };
            Database.Insert(a);
            DateTime start = DateTime.Now;
            List<Guid> ids = new List<Guid>();
            for (int i = 0; i < Count; i++)
            {
                Animal a2 = new Animal { Name = "Name" + i };
                Database.Insert(a2);
                ids.Add(a2.Id);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void GuidKey_UsingReturnValue()
        {
            Animal a = new Animal { Name = "Name" };
            Database.Insert(a);
            DateTime start = DateTime.Now;
            List<Guid> ids = new List<Guid>();
            for (int i = 0; i < Count; i++)
            {
                Animal a2 = new Animal { Name = "Name" + i };
                var id = Database.Insert(a2);
                ids.Add(id);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void AssignKey_UsingEntity()
        {
            Car ca = new Car { Id = string.Empty.PadLeft(15, '0'), Name = "Name" };
            Database.Insert(ca);
            DateTime start = DateTime.Now;
            List<string> ids = new List<string>();
            for (int i = 0; i < Count; i++)
            {
                var key = (i + 1).ToString().PadLeft(15, '0');
                Car ca2 = new Car { Id = key, Name = "Name" + i };
                Database.Insert(ca2);
                ids.Add(ca2.Id);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }

        [Test]
        public void AssignKey_UsingReturnValue()
        {
            Car ca = new Car { Id = string.Empty.PadLeft(15, '0'), Name = "Name" };
            Database.Insert(ca);
            DateTime start = DateTime.Now;
            List<string> ids = new List<string>();
            for (int i = 0; i < Count; i++)
            {
                var key = (i + 1).ToString().PadLeft(15, '0');
                Car ca2 = new Car { Id = key, Name = "Name" + i };
                var id = Database.Insert(ca2);
                ids.Add(id);
            }

            double total = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Total Time:" + total);
            Console.WriteLine("Average Time:" + total / Count);
        }
    }
}
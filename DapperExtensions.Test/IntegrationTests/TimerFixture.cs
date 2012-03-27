using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Data;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests
{
    [TestFixture]
    public class TimerFixture : IntegrationBaseFixture
    {
        private static int cnt = 1000;

        public class InsertTimes : IntegrationBaseFixture
        {
            [Test]
            public void IdentityKey_UsingEntity()
            {
                RunTest(c =>
                            {
                                Person p = new Person
                                               {
                                                   FirstName = "FirstName",
                                                   LastName = "LastName",
                                                   DateCreated = DateTime.Now,
                                                   Active = true
                                               };
                                c.Insert(p);
                                DateTime start = DateTime.Now;
                                List<int> ids = new List<int>();
                                for (int i = 0; i < cnt; i++)
                                {
                                    Person p2 = new Person
                                                    {
                                                        FirstName = "FirstName" + i,
                                                        LastName = "LastName" + i,
                                                        DateCreated = DateTime.Now,
                                                        Active = true
                                                    };
                                    c.Insert(p2);
                                    ids.Add(p2.Id);
                                }

                                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                                Console.WriteLine("Total Time:" + total);
                                Console.WriteLine("Average Time:" + total/cnt);
                            });
            }

            [Test]
            public void IdentityKey_UsingReturnValue()
            {
                RunTest(c =>
                            {
                                Person p = new Person
                                               {
                                                   FirstName = "FirstName",
                                                   LastName = "LastName",
                                                   DateCreated = DateTime.Now,
                                                   Active = true
                                               };
                                c.Insert(p);
                                DateTime start = DateTime.Now;
                                List<int> ids = new List<int>();
                                for (int i = 0; i < cnt; i++)
                                {
                                    Person p2 = new Person
                                                    {
                                                        FirstName = "FirstName" + i,
                                                        LastName = "LastName" + i,
                                                        DateCreated = DateTime.Now,
                                                        Active = true
                                                    };
                                    var id = c.Insert(p2);
                                    ids.Add(id);
                                }

                                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                                Console.WriteLine("Total Time:" + total);
                                Console.WriteLine("Average Time:" + total/cnt);
                            });
            }

            [Test]
            public void GuidKey_UsingEntity()
            {
                RunTest(c =>
                            {
                                Animal a = new Animal {Name = "Name"};
                                c.Insert(a);
                                DateTime start = DateTime.Now;
                                List<Guid> ids = new List<Guid>();
                                for (int i = 0; i < cnt; i++)
                                {
                                    Animal a2 = new Animal {Name = "Name" + i};
                                    c.Insert(a2);
                                    ids.Add(a2.Id);
                                }

                                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                                Console.WriteLine("Total Time:" + total);
                                Console.WriteLine("Average Time:" + total/cnt);
                            });
            }

            [Test]
            public void GuidKey_UsingReturnValue()
            {
                RunTest(c =>
                            {
                                Animal a = new Animal {Name = "Name"};
                                c.Insert(a);
                                DateTime start = DateTime.Now;
                                List<Guid> ids = new List<Guid>();
                                for (int i = 0; i < cnt; i++)
                                {
                                    Animal a2 = new Animal {Name = "Name" + i};
                                    var id = c.Insert(a2);
                                    ids.Add(id);
                                }

                                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                                Console.WriteLine("Total Time:" + total);
                                Console.WriteLine("Average Time:" + total/cnt);
                            });
            }

            [Test]
            public void AssignKey_UsingEntity()
            {
                RunTest(c =>
                            {
                                Car ca = new Car {Id = string.Empty.PadLeft(15, '0'), Name = "Name"};
                                c.Insert(ca);
                                DateTime start = DateTime.Now;
                                List<string> ids = new List<string>();
                                for (int i = 0; i < cnt; i++)
                                {
                                    var key = (i + 1).ToString().PadLeft(15, '0');
                                    Car ca2 = new Car {Id = key, Name = "Name" + i};
                                    c.Insert(ca2);
                                    ids.Add(ca2.Id);
                                }

                                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                                Console.WriteLine("Total Time:" + total);
                                Console.WriteLine("Average Time:" + total/cnt);
                            });
            }

            [Test]
            public void AssignKey_UsingReturnValue()
            {
                RunTest(c =>
                            {
                                Car ca = new Car {Id = string.Empty.PadLeft(15, '0'), Name = "Name"};
                                c.Insert(ca);
                                DateTime start = DateTime.Now;
                                List<string> ids = new List<string>();
                                for (int i = 0; i < cnt; i++)
                                {
                                    var key = (i + 1).ToString().PadLeft(15, '0');
                                    Car ca2 = new Car {Id = key, Name = "Name" + i};
                                    var id = c.Insert(ca2);
                                    ids.Add(id);
                                }

                                double total = DateTime.Now.Subtract(start).TotalMilliseconds;
                                Console.WriteLine("Total Time:" + total);
                                Console.WriteLine("Average Time:" + total/cnt);
                            });
            }
        }
    }
}
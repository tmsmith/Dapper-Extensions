using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Extensions;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures
{
    public abstract partial class FixturesBase
    {
        [Test]
        public void GetMultiple_DoesNotDuplicate()
        {
            List<Guid> list = new List<Guid>();
            for (int i = 0; i < 1000; i++)
            {
                Guid id = DapperExtensions.GetNextGuid();
                Assert.IsFalse(list.Contains(id));
                list.Add(id);
            }
        }
    }
}

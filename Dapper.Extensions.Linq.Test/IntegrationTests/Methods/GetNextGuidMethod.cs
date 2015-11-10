using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Extensions;
using Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Methods
{

    [TestFixture]
    public class GetNextGuidMethod : SqlServerBase
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
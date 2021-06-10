using FluentAssertions;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace DapperExtensions.Test.IntegrationTests.Oracle
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class NonCrudFixture
    {
        [TestFixture]
        public class EnableCaseInsensitiveMethod : OracleBaseFixture
        {
            [Test]
            public void GetIdentity_ThrowsException()
            {
                Dialect.EnableCaseInsensitive(Db.Connection);

                var info = (Db.Connection as OracleConnection).GetSessionInfo();

                info.Sort.Should().Be("BINARY_CI");
                info.Comparison.Should().Be("LINGUISTIC");
            }
        }
    }
}
#if NETCOREAPP
using Dapper;
using DapperExtensions.Sql;
#if NET50
using IBM.Data.Db2;
#elif NET60
using IBM.Data.Db2;
#else
using IBM.Data.DB2.Core;
#endif
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.Async.DB2
{
    [NonParallelizable]
    public class DB2BaseAsyncFixture : DatabaseAsyncTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            var connection = new DB2Connection(ConnectionString("DB2"));

            CommonSetup(connection, new DB2Dialect());

            ExecuteScripts(Db.Connection, false, "DropAnimalTable", "DropFooTable", "DropMultikeyTable", "DropPersonTable", "DropCarTable");
            ExecuteScripts(Db.Connection, true, CreateTableScripts);
        }
    }
}
#endif
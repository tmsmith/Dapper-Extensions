#if NETCOREAPP
using DapperExtensions.Sql;
#if NETCORE
using IBM.Data.DB2.Core;
#elif NET50
using IBM.Data.Db2;
#elif NET60
using IBM.Data.Db2;
#else
using IBM.Data.Db2;
#endif
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.DB2
{
    [NonParallelizable]
    public class DB2BaseFixture : DatabaseTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            ConnectionString = GetConnectionString("DB2");
            var connection = new DB2Connection(ConnectionString);

            CommonSetup(connection, new DB2Dialect());

            ExecuteScripts(Db.Connection, false, "DropAnimalTable", "DropFooTable", "DropMultikeyTable", "DropPersonTable", "DropCarTable", "DropBarTable");
            ExecuteScripts(Db.Connection, true, CreateTableScripts);
        }
    }
}
#endif
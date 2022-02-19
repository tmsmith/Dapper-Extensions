using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace DapperExtensions.Test.IntegrationTests.Async
{
    [NonParallelizable]
    public abstract class DatabaseAsyncTestsFixture : DatabaseTestsFixture
    {
        protected DatabaseAsyncTestsFixture(string configPath = null) : base(configPath)
        {
        }

        public new IAsyncDatabase Db { get; private set; }

        protected override void CommonSetup(DbConnection connection, SqlDialectBase sqlDialect)
        {
            Dialect = sqlDialect;
            var config = DapperAsyncExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), sqlDialect);
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new AsyncDatabase(connection, sqlGenerator);
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                Db.Dispose();
            }
        }
    }
}
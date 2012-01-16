using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using NUnit.Framework;

namespace DapperExtensions.Test.Helpers
{
    public class DatabaseConnection
    {
        protected IDbConnection Connection;
        protected DapperExtensions.IDapperExtensionsImpl Impl;

        [SetUp]
        public virtual void Setup()
        {
            string databaseName = "db_" + Guid.NewGuid().ToString() + ".sdf";
            TestHelpers.LoadDatabase(databaseName);
            Connection = TestHelpers.GetConnection(databaseName);
            Impl = new DapperExtensions.DapperExtensionsImpl(typeof(AutoClassMapper<>), new SqlGeneratorImpl(new SqlCeDialect()));
        }

        [TearDown]
        public virtual void Teardown()
        {
            string db = Connection.Database;
            Connection.Close();
            Connection.Dispose();
            TestHelpers.DeleteDatabase(db);
        }

    }
}
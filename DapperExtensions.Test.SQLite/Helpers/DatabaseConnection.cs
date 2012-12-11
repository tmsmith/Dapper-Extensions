using System;
using System.Data;
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
            string databaseName = string.Format("db_{0}.s3db", Guid.NewGuid().ToString());
            TestHelpers.LoadDatabase(databaseName);
            Connection = TestHelpers.GetConnection(databaseName);
            Impl = new DapperExtensions.DapperExtensionsImpl(typeof(AutoClassMapper<>), TestHelpers.GetGenerator());
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
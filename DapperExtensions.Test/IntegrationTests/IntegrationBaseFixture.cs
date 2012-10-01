using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests
{
    public class IntegrationBaseFixture
    {
        protected readonly IList<Database> Databases = new List<Database>();
        protected DapperExtensions.IDapperExtensionsImpl Impl;

        [SetUp]
        public virtual void Setup()
        {
            Databases.Add(TestHelpers.GetSqlConnection("Data Source=.;Initial Catalog=dapperTest;Integrated security=True;"));
            Databases.Add(TestHelpers.GetSqlCeConnection("Data Source=.\\dapperTest.sdf"));
            //Databases.Add(TestHelpers.GetSqliteConnecton("Data Source=.\\dapperTest.sqlite"));
            //Databases.Add(TestHelpers.GetMySqlConnection("Server=localhost;Port=3306;Database=dapperTest;uid=root;password=password!"));
            TestHelpers.LoadDatabases(Databases);
            DapperExtensions.DefaultMapper = typeof(AutoClassMapper<>);
        }

        [TearDown]
        public virtual void Teardown()
        {
            foreach (var database in Databases)
            {
                string databaseName = database.Connection.Database;
                database.Connection.Close();
                database.Connection.Dispose();
                if (database.Dialect.GetType() == typeof(SqlCeDialect) || database.Dialect.GetType() == typeof(SqliteDialect))
                {
                    TestHelpers.DeleteDatabase(databaseName);
                }
            }

            Databases.Clear();
        }

        public virtual void RunTest(Action<IDbConnection> action)
        {
            foreach (var database in Databases)
            {
                Console.WriteLine("Running against " + database.Dialect.GetType().Name);
                DapperExtensions.SqlDialect = database.Dialect;
                action(database.Connection);
            }
        }
    }
}
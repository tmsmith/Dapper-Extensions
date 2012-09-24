using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using MySql.Data.MySqlClient;
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
            Databases.Add(GetSqlConnection("Data Source=.;Initial Catalog=dapperTest;Integrated security=True;"));
            Databases.Add(GetSqlCeConnection("Data Source=.\\dapperTest.sdf"));
            //Databases.Add(GetMySqlConnection("Server=localhost;Port=3306;Database=dapperTest;uid=root;password=password!"));

            foreach (var database in Databases)
            {
                foreach (var setupFile in database.SetupFiles)
                {
                    database.Connection.Execute(setupFile);
                }
            }

            DapperExtensions.DefaultMapper = typeof(AutoClassMapper<>);
        }

        [TearDown]
        public virtual void Teardown()
        {
            foreach (var database in Databases)
            {
                database.Connection.Close();
                database.Connection.Dispose();
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

        protected virtual Database GetSqlConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return new Database
                       {
                           Connection = connection,
                           Dialect = new SqlServerDialect(),
                           SetupFiles = GetSetupFiles("SqlServer")
                       };
        }

        protected virtual Database GetSqlCeConnection(string connectionString)
        {
            string[] connectionParts = connectionString.Split(';');
            string file = connectionParts
                .ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1])
                .Where(d => d.Key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                .Select(k => k.Value).Single();

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (SqlCeEngine ce = new SqlCeEngine(connectionString))
            {
                ce.CreateDatabase();
            }

            SqlCeConnection connection = new SqlCeConnection(connectionString);
            connection.Open();
            return new Database
                       {
                           Connection = connection,
                           Dialect = new SqlCeDialect(),
                           SetupFiles = GetSetupFiles("SqlCe")
                       };
        }

        protected virtual Database GetMySqlConnection(string connectionString)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            return new Database
                       {
                           Connection = connection,
                           Dialect = new MySqlDialect(),
                           SetupFiles = GetSetupFiles("MySql")
                       };
        }

        protected List<string> GetSetupFiles(string prefix)
        {
            return new List<string>()
                       {
                           ReadScriptFile(prefix + ".CreateAnimalTable"),
                           ReadScriptFile(prefix + ".CreateFooTable"),
                           ReadScriptFile(prefix + ".CreateMultikeyTable"),
                           ReadScriptFile(prefix + ".CreatePersonTable"),
                           ReadScriptFile(prefix + ".CreateCarTable")
                       };
        }

        protected virtual string ReadScriptFile(string name)
        {
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DapperExtensions.Test.IntegrationTests.SqlScripts." + name + ".sql"))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        protected class Database
        {
            public IDbConnection Connection { get; set; }
            public ISqlDialect Dialect { get; set; }
            public IList<string> SetupFiles { get; set; }
        }
    }
}
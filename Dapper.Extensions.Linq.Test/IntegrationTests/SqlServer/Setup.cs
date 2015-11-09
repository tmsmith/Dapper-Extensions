using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Database;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Sql;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    [SetUpFixture]
    public class SqlServerTests
    {
        const string DatabaseName = "dapperTest";
        protected IDatabase Database;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            using(var sqlConnection = new SqlConnection("Data Source=.;Integrated security=True;"))
            {
                const string sqlCreateDatabase = @"
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{0}')
                BEGIN
                    CREATE DATABASE {0};
                END
                ";
                sqlConnection.Execute(string.Format(sqlCreateDatabase, DatabaseName));
            }

            var connection = new SqlConnection(string.Format("Data Source=.;Initial Catalog={0};Integrated security=True;", DatabaseName));
            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect());

            //DapperConfiguration
            //    .Use()
            //    .UseClassMapper(typeof(AutoClassMapper<>))
            //    .Build();

            var sqlGenerator = new SqlGeneratorImpl(config);
            Database = new Database(connection, sqlGenerator);
            var files = new List<string>
                {
                    ReadScriptFile("CreateAnimalTable"),
                    ReadScriptFile("CreateFooTable"),
                    ReadScriptFile("CreateMultikeyTable"),
                    ReadScriptFile("CreatePersonTable"),
                    ReadScriptFile("CreateCarTable")
                };

            foreach (var setupFile in files)
            {
                connection.Execute(setupFile);
            }
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
        }

        private string ReadScriptFile(string name)
        {
            string fileName = GetType().Namespace + ".Sql." + name + ".sql";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
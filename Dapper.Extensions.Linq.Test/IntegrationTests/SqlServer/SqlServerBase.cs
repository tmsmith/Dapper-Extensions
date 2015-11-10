using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Dapper.Extensions.Linq.CastleWindsor;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Sql;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class SqlServerBase
    {
        const string DatabaseName = "dapperTest";
        protected Castle.Windsor.WindsorContainer Container;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            using (var sqlConnection = new SqlConnection("Data Source=.;Integrated security=True;"))
            {
                const string sqlCreateDatabase = @"
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{0}')
                BEGIN
                    CREATE DATABASE {0};
                END
                ";
                sqlConnection.Execute(string.Format(sqlCreateDatabase, DatabaseName));
            }

            Container = new Castle.Windsor.WindsorContainer();

            DapperConfiguration
                .Use()
                .UseClassMapper(typeof(AutoClassMapper<>))
                .UseContainer<WindsorContainer>(cfg => cfg.UseExisting(Container))
                .UseSqlDialect(new SqlServerDialect())
                .FromAssembly("Dapper.Extensions.Linq.Test")
                .FromAssembly("Dapper.Extensions.Linq.Test.Entities")
                .FromAssembly("Dapper.Extensions.Linq.Test.Maps")
                .Build();

            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["__Default"].ConnectionString);
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
        public void RunAfterAnyTests() { }

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
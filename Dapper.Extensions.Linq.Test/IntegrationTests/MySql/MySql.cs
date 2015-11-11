using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using Dapper.Extensions.Linq.CastleWindsor;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.MySql;
using Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.MySql
{
    public class MySql : FixturesBase
    {
        const string DatabaseName = "dapperTest";

        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            using (var mySqlConnection = new MySqlConnection("Server=localhost;Port=3306;uid=root;password=password!"))
            {
                mySqlConnection.Execute(string.Format("CREATE DATABASE IF NOT EXISTS `{0}`", DatabaseName));
            }

            Container = new Castle.Windsor.WindsorContainer();

            DapperConfiguration
                .Use()
                .UseClassMapper(typeof(AutoClassMapper<>))
                .UseContainer<ContainerForWindsor>(cfg => cfg.UseExisting(Container))
                .UseSqlDialect(new MySqlDialect())
                .WithDefaultConnectionStringNamed("__DefaultMySql")
                .FromAssembly("Dapper.Extensions.Linq.Test.Entities")
                .FromAssembly("Dapper.Extensions.Linq.Test.Maps")
                .Build();

            var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["__DefaultMySql"].ConnectionString);
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

        private string ReadScriptFile(string name)
        {
            string fileName = GetType().Namespace + ".Sql." + name + ".sql";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        [TestFixtureTearDown]
        public void TearDown() { }

    }
}
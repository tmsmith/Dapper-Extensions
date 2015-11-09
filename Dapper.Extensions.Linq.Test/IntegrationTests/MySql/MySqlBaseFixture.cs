using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Database;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Sql;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.MySql
{
    public class MySqlBaseFixture
    {
        const string DatabaseName = "dapperTest";
        protected IDatabase Database;
        private MySqlConnection _connection;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            _connection = new MySqlConnection("Server=localhost;Port=3306;uid=root;password=password!");
            _connection.Execute(string.Format("CREATE DATABASE IF NOT EXISTS `{0}`", DatabaseName));

            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
            var sqlGenerator = new SqlGeneratorImpl(config);
            var connection = new MySqlConnection(string.Format("Server=localhost;Port=3306;Database={0};uid=root;password=password!", DatabaseName));

            Database = new Database(connection, sqlGenerator);

            var files = new List<string>
            {
                ReadScriptFile("CreateFooTable"),
                ReadScriptFile("CreateMultikeyTable"),
                ReadScriptFile("CreatePersonTable"),
                ReadScriptFile("CreateCarTable"),
                ReadScriptFile("CreateAnimalTable")
            };

            foreach (var setupFile in files)
            {
                connection.Execute(setupFile);
            }
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            _connection.Execute(string.Format("DROP DATABASE {0}", DatabaseName));
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
    }
}
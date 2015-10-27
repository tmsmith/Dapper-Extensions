using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dapper;
using Linq.Dapper.Extensions.Mapper;
using Linq.Dapper.Extensions.Sql;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Linq.Dapper.Extensions.Test.IntegrationTests.MySql
{
    public class MySqlBaseFixture
    {
        protected IDatabase Db;

        [SetUp]
        public virtual void Setup()
        {
            var connection = new MySqlConnection("Server=localhost;Port=3306;Database=dapperTest;uid=root;password=password!");
            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new Database(connection, sqlGenerator);
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

        public string ReadScriptFile(string name)
        {
            string fileName = GetType().Namespace + ".Sql." + name + ".sql";
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
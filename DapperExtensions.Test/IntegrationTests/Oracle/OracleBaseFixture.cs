using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace DapperExtensions.Test.IntegrationTests.Oracle
{
    public class OracleBaseFixture
    {
        protected IDatabase Db;

        [SetUp]
        public virtual void Setup()
        {
            var connection = new OracleConnection("Data Source=localhost/xe; User Id=xe; Password=xe;");
            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new OracleDialect());
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new Database(connection, sqlGenerator);
            var files = new List<string>
                                {
                                    ReadScriptFile("CreateAnimalTable"),
                                    ReadScriptFile("CreatePersonTable"),
                                    ReadScriptFile("CreateCarTable")
                                };

            foreach (var setupFile in files)
            {
                connection.Execute(setupFile);
            }
        }

        public string ReadScriptFile(string name)
        {
            string fileName = GetType().Namespace + ".Sql." + name + ".sql";
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
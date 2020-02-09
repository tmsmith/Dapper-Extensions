using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using IBM.Data.DB2;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.DB2
{
    public class DB2BaseFixture
    {
        protected IDatabase Db;

        [SetUp]
        public virtual void Setup()
        {
            var connection = new DB2Connection("Server=localhost;Database=test;UID=db2admin;PWD=db2admin;CurrentSchema=db2admin;");
            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new DB2Dialect());
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new Database(connection, sqlGenerator);
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
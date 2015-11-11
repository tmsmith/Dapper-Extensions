using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.CastleWindsor;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.SqlCe;
using Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlCe
{
    public class SqlCe : FixturesBase
    {
        private SqlCeConnection _connection;

        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["__DefaultSqlCe"].ConnectionString;
            string[] connectionParts = connectionString.Split(';');
            string file = connectionParts
                .ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1])
                .Where(d => d.Key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                .Select(k => k.Value).Single();

            if (File.Exists(file))
                File.Delete(file);

            using (SqlCeEngine ce = new SqlCeEngine(connectionString))
            {
                ce.CreateDatabase();
            }

            Container = new Castle.Windsor.WindsorContainer();

            DapperConfiguration
                .Use()
                .UseClassMapper(typeof(AutoClassMapper<>))
                .UseContainer<WindsorContainer>(cfg => cfg.UseExisting(Container))
                .UseSqlDialect(new SqlCeDialect())
                .WithDefaultConnectionStringNamed("__DefaultSqlCe")
                .FromAssembly("Dapper.Extensions.Linq.Test.Entities")
                .FromAssembly("Dapper.Extensions.Linq.Test.Maps")
                .Build();

            _connection = new SqlCeConnection(connectionString);
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
                _connection.Execute(setupFile);
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
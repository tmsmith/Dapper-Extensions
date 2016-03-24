﻿using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Dapper.Extensions.Linq.CastleWindsor;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Sql;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public static class SqlSetup
    {
        const string DatabaseName = "dapperTest";

        public static Castle.Windsor.WindsorContainer Configuration()
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

            var container = new Castle.Windsor.WindsorContainer();

            DapperConfiguration
                .Use()
                .UseClassMapper(typeof(AutoClassMapper<>))
                .UseContainer<ContainerForWindsor>(cfg => cfg.UseExisting(container))
                .UseSqlDialect(new SqlServerDialect())
                .WithDefaultConnectionStringNamed("__DefaultSqlServer")
                .FromAssembly("Dapper.Extensions.Linq.Test.Entities")
                .FromAssembly("Dapper.Extensions.Linq.Test.Maps")
                .Build();

            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["__DefaultSqlServer"].ConnectionString);
            var files = new List<string>
            {
                ReadScriptFile("CreateAnimalTable"),
                ReadScriptFile("CreateFooTable"),
                ReadScriptFile("CreateMultikeyTable"),
                ReadScriptFile("CreatePersonTable"),
                ReadScriptFile("CreateCarTable"),
                ReadScriptFile("CreatePhoneTable")

            };

            foreach (var setupFile in files)
            {
                connection.Execute(setupFile);
            }

            return container;
        }

        private static string ReadScriptFile(string name)
        {
            string fileName = typeof(SqlSetup).Namespace + ".Sql." + name + ".sql";
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